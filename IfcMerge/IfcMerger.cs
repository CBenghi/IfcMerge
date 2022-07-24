using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xbim.Common;
using Xbim.Common.Metadata;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

namespace IfcMerge
{
	public class IfcMerger
	{
       
        public IfcMerger(Options opts)
        {
            this.opts = opts;
        }

        internal int processed = 0;

		IfcStore? destModel = null;
        private readonly Options opts;
        

        XbimInstanceHandle? sharedOwner = null;
        XbimInstanceHandle? sharedProject = null;
        readonly Dictionary<UnifiedEntity, XbimInstanceHandle> spatialNames = new();

        object? SemanticFilter(Xbim.Common.Metadata.ExpressMetaProperty property, object parentObject)
        {
            var prop = property.PropertyInfo.GetValue(parentObject, null);
            if (prop != null)
            {
                if (parentObject is IPersistEntity pe)
                {
                    Debug.WriteLine($"Prop: {property.Name} of {parentObject.GetType().Name} #{pe.EntityLabel}");
                }
                else
                {
                    Debug.WriteLine($"Prop: {property.Name} of {parentObject.GetType().Name}");
                }
            }            
            // this just does not drop anything
            return prop;

        }

        XbimInstanceHandleMap? map;

        internal void MergeFile(FileInfo f)
		{
            using (var model = IfcStore.Open(f.FullName))
            {
                var elements = model.Instances.OfType<IIfcElement>();
                if (destModel is null)
                {
                    destModel = IfcStore.Create(model.SchemaVersion, Xbim.IO.XbimStoreType.InMemoryModel);
                    if (destModel is null)
                        throw new System.Exception("Could not create model.");
                }

                using var txn = destModel.BeginTransaction("Insert copy");
                //single map should be used for all insertions between two models
                map = new XbimInstanceHandleMap(model, destModel);
                SetAllShared(model);
                foreach (var element in elements)
                {
                    destModel.InsertCopy(element, map, SemanticFilter, true, false);
                }
                txn.Commit();
                GetAllShared();
            };
		}

        private void SetAllShared(IfcStore model)
        {
            if (map is null)
                return;
            // project
            SetShared<IIfcProject>(model, sharedProject);
            // contexts
            foreach (var ctx in model.Instances.OfType<IIfcRepresentationContext>())
            {
                var t = new UnifiedEntity(ctx);
                if (spatialNames.TryGetValue(t, out var found))
                    map.Add(new XbimInstanceHandle(ctx), found);
            }

            // owner
            if (opts.MergeOwner)
                SetShared<IIfcOwnerHistory>(model, sharedOwner);
            // spatial
            if (opts.MergeSpatial)
            {
                foreach (var namedSpace in model.Instances.OfType<IIfcSpatialElement>())
                {
                    var t = new UnifiedEntity(namedSpace);
                    if (spatialNames.TryGetValue(t, out var found))
                        map.Add(new XbimInstanceHandle(namedSpace), found);
                }
            }
        }

        private void SetShared<T>(IfcStore model, XbimInstanceHandle? sharedHandle) where T : IPersistEntity
        {
            if (sharedHandle is not null)
            {
                foreach (var prj in model.Instances.OfType<T>())
                {
                    map.Add(new XbimInstanceHandle(prj), sharedHandle.Value);
                }
            }
        }

        private void GetAllShared()
        {
            if (destModel is null)
                return;
            if (sharedProject is null)
                sharedProject = GetShared<IIfcProject>();

            // context of the project are retained
            foreach (var context in destModel.Instances.OfType<IIfcRepresentationContext>())
            {
                var t = new UnifiedEntity(context);
                if (!spatialNames.ContainsKey(t))
                    spatialNames.Add(t, new XbimInstanceHandle(context));
            }

            // owner history
            if (opts.MergeOwner && sharedOwner is null)
                sharedOwner = GetShared<IIfcOwnerHistory>();
            // spatial
            if (opts.MergeSpatial)
            {
                foreach (var namedSpace in destModel.Instances.OfType<IIfcSpatialElement>())
                {
                    var t = new UnifiedEntity(namedSpace);
                    if (!spatialNames.ContainsKey(t))
                        spatialNames.Add(t, new XbimInstanceHandle(namedSpace));
                }
            }

        }

        private XbimInstanceHandle? GetShared<T>() where T : IPersistEntity
        {
            if (destModel is null)
                return null;
            var t = destModel.Instances.OfType<T>().FirstOrDefault();
            if (t is not null)
                return new XbimInstanceHandle(t);
            return null;
        }

        internal FileInfo SaveIfcModel()
		{
			var outF = new FileInfo(opts.OutputFile);
			if (destModel != null)
            {
				destModel.SaveAs(outF.FullName);
				destModel.Dispose();
				destModel = null;
			}
			return outF;
		}
	}
}
