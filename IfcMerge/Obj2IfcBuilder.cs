﻿using FileFormatWavefront.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xbim.Common;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc4.GeometricConstraintResource;
using Xbim.Ifc4.GeometricModelResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.RepresentationResource;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.Ifc4.TopologyResource;
using Xbim.IO;

namespace Obj2Ifc
{
    public class Obj2IfcBuilder
    {
        private IList<Source> _sources = new List<Source>();

        public int SceneCount { get => _sources.Count;  }

        public void AddObjScene(Source scene)
        {
            _sources.Add(scene);
        }

        public class Source
        {
            public Scene Scene;
            public FileInfo SourceFile;

            public Source(Scene model, FileInfo f)
            {
                Scene = model;
                SourceFile = f;
            }
        }

        public string CreateIfcModel(Options opts)
        {

            using (var model = InitialiseIfcModel(opts))
            {
                if (model != null)
                {
                    IfcSpatialStructureElement container = opts.SimplifySpatial
                        ? CreateBuilding(model, opts)
                        : CreateStoreyHierarchy(model, opts) as IfcSpatialStructureElement;


                    IList<IfcProduct> products = new List<IfcProduct>();
                    foreach (var scene in _sources)
                    {
                        var sceneProducts = CreateProducts(model, scene, opts);
                        foreach(var product in sceneProducts)
                        {
                            products.Add(product);
                        }
                    }

                    using (var txn = model.BeginTransaction("Add products"))
                    {
                        foreach(var product in products)
                        {
                            container.AddElement(product);
                        }
                        txn.Commit();
                    }

                    var ifcFile = string.IsNullOrEmpty(opts.IfcFile) ? Path.ChangeExtension(opts.ObjFiles.First(), "ifc") : opts.IfcFile;

                    var format = StorageType.Ifc;
                    switch (Path.GetExtension(ifcFile).ToLowerInvariant())
                    {
                        case "ifczip":
                            format = StorageType.IfcZip;
                            break;
                        case "ifcxml":
                            format = StorageType.IfcXml;
                            break;
                        default:
                            break;
                    }
                    //write the Ifc File
                    model.SaveAs(ifcFile, format);

                    return ifcFile;

                }
                else
                {
                    Console.WriteLine("Failed to initialise the model");
                }
                return "";
            }

        }

        private IEnumerable<IfcProduct> CreateProducts(IfcStore model, Source source, Options opts)
        {
            using (var txn = model.BeginTransaction("Create Product"))
            {
                var product = model.Instances.New<IfcBuildingElementProxy>();
                product.Name = source.Scene.ObjectName ?? // obj name in scene prevails if defined
                    (opts.UseObjFileName ? Path.GetFileNameWithoutExtension(source.SourceFile.FullName) : "Obj Object"); // if option is set, adopt file name
                IfcGeometricRepresentationItem geometry;
                string representationType;

                switch (opts.GeometryMode)
                {
                    case GeometryMode.TriangulatedFaceSet:
                        geometry = CreateTriangulatedFaceSet(model, source.Scene);
                        representationType = "Tessellation";
                        break;
                    case GeometryMode.FacetedBrep:
                        geometry = CreateFacetedBrep(model, source.Scene);
                        representationType = "Brep";
                        break;

                    default:
                        throw new NotImplementedException($"Geometry mode not implemented {opts.GeometryMode}");
                }

                if (opts.FullProperties.Any())
                {
                    // prepare pset
                    var pset = CreatePset(model, opts);
                    // add relation
                    model.Instances.New<IfcRelDefinesByProperties>(rel =>
                    {
                        rel.RelatedObjects.Add(product);
                        rel.RelatingPropertyDefinition = pset;
                    });
                 }


                //Create a Definition shape to hold the geometry
                var shape = model.Instances.New<IfcShapeRepresentation>();
                var modelContext = model.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault(c => c.ContextType == "Model");
                shape.ContextOfItems = modelContext;
                shape.RepresentationType = representationType;
                shape.RepresentationIdentifier = "Body";
                shape.Items.Add(geometry);

                //Create a Product Definition and add the model geometry to the wall
                var rep = model.Instances.New<IfcProductDefinitionShape>();
                rep.Representations.Add(shape);
                product.Representation = rep;

                //parameters to insert the geometry in the model
                var origin = model.Instances.New<IfcCartesianPoint>();
                origin.SetXYZ(0, 0, 0);

                //now place the element into the model
                var lp = model.Instances.New<IfcLocalPlacement>();
                var ax3D = model.Instances.New<IfcAxis2Placement3D>();
                ax3D.Location = origin;
                ax3D.Axis = model.Instances.New<IfcDirection>();
                ax3D.Axis.SetXYZ(0, 0, 1); // axis is z direction
                ax3D.RefDirection = model.Instances.New<IfcDirection>();
                ax3D.RefDirection.SetXYZ(1, 0, 0); // ref-direction is along x
                lp.RelativePlacement = ax3D;
                product.ObjectPlacement = lp;

                txn.Commit();
                return new IfcProduct[] { product };
            }
        }

        private static IIfcPropertySet CreatePset(IfcStore model, Options opts)
        {
            var properties = new List<IfcProperty>();
            foreach (var prop in opts.FullProperties)
            {
                var thisp = model.Instances.New<IfcPropertySingleValue>();
                thisp.Name = prop.Name;
                switch (prop.Type)
                {
                    case Options.Property.PropType.Label:
                        thisp.NominalValue = new IfcLabel((string)prop.Value);
                        break;
                    case Options.Property.PropType.Real:
                        thisp.NominalValue = new IfcReal((double)prop.Value);
                        break;
                    default:
                        break;
                }
                properties.Add(thisp);
            }

            // create and populate property set
            var pset = model.Instances.New<IfcPropertySet>();
            pset.Name = opts.PsetName;
            pset.HasProperties.AddRange(properties);
            return pset;
        }

        private static IfcFacetedBrep CreateFacetedBrep(IfcStore model, Scene scene)
        {
            // prepare points
     
            int i = 0;
            var vertexMap = new Dictionary<int, IfcCartesianPoint>();
            foreach (var vertex in scene.Vertices)
            {
                var point = model.Instances.New<IfcCartesianPoint>(pt =>
                   {
                       pt.X = new IfcLengthMeasure(vertex.x);
                       pt.Y = new IfcLengthMeasure(vertex.y);
                       pt.Z = new IfcLengthMeasure(vertex.z);
                   });
                vertexMap.Add(i++, point);
            }

            // prepare return 
            var shell = model.Instances.New<IfcClosedShell>();
            var ret = model.Instances.New<IfcFacetedBrep>(r=>r.Outer = shell);

            // add faces
            i = 0;
            foreach (var face in GetAllFaces(scene))
            {
                var loop = model.Instances.New<IfcPolyLoop>();
                foreach (var zeroBasedIndex in face.Indices)
                {
                    loop.Polygon.Add(vertexMap[zeroBasedIndex.vertex]);
                }
                var outerB = model.Instances.New<IfcFaceOuterBound>(ob =>
                {
                    ob.Bound = loop;
                    ob.Orientation = new IfcBoolean(true);
                });
                var Ifcface = model.Instances.New<IfcFace>(fc =>
                {
                    fc.Bounds.Add(outerB);
                });
                shell.CfsFaces.Add(Ifcface);
            }

            return ret;
        }

        private static IfcTriangulatedFaceSet CreateTriangulatedFaceSet(IfcStore model, Scene scene)
        {
            var coords = model.Instances.New<IfcCartesianPointList3D>();
            var faceSet = model.Instances.New<IfcTriangulatedFaceSet>(fs =>
            {
                fs.Closed = false;
                fs.Coordinates = coords;
            });

            int i = 0;
            foreach (var vertex in scene.Vertices)
            {
                var ifcVertex = new[] { 
                    new IfcLengthMeasure(vertex.x), 
                    new IfcLengthMeasure(vertex.y), 
                    new IfcLengthMeasure(vertex.z) 
                };
                coords.CoordList.GetAt(i).AddRange(ifcVertex);
                i++;
            }

            i = 0;
            foreach (var face in GetAllFaces(scene))
            {
                var indices = face.Indices.Select(v => new IfcPositiveInteger(v.vertex + 1));
                faceSet.CoordIndex.GetAt(i).AddRange(indices);
                i++;
            }

            return faceSet;
        }

        private static IEnumerable<Face> GetAllFaces(Scene scene)
        {
            var t = scene.UngroupedFaces.ToList();
            foreach (var grp in scene.Groups)
            {
                if (grp.Faces != null)
                    t.AddRange(grp.Faces);
            }
            return t;
        }

        private IfcStore InitialiseIfcModel(Options opts)
        {
            //first we need to set up some credentials for ownership of data in the new model
            var credentials = new XbimEditorCredentials
            {
                ApplicationDevelopersName = "xbim Ltd",
                ApplicationFullName = "Xbim Obj2Ifc",
                ApplicationIdentifier = "Xbim.Obj2Ifc.exe",
                ApplicationVersion = "1.0",
                EditorsFamilyName = "Team",
                EditorsGivenName = "xbim",
                EditorsOrganisationName = "xbim Ltd"
            };
            //now we can create an IfcStore, it is in Ifc4 format and will be held in memory rather than in a database

            var model = IfcStore.Create(credentials, XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel);

            //Begin a transaction as all changes to a model are ACID
            using (var txn = model.BeginTransaction("Initialise Model"))
            {
                //create a project
                var project = model.Instances.New<IfcProject>();
                //set the units to SI (mm and metres)
                project.Initialize(ProjectUnits.SIUnitsUK);
                if (opts.LenghtUnit != LenghtUnits.MilliMeters)
                    SetUnits(project, opts.LenghtUnit);
             
                project.Name = opts.ProjectName ?? "Default Project";
                //now commit the changes, else they will be rolled back at the end of the scope of the using statement
                txn.Commit();
            }
            return model;
        }

        private void SetUnits(IfcProject project, LenghtUnits lenghtUnit)
        {
            var t = project.UnitsInContext;
            switch (lenghtUnit)
            {
                case LenghtUnits.MilliMeters:
                    break;
                case LenghtUnits.Meters:
                    t.SetOrChangeSiUnit(IfcUnitEnum.LENGTHUNIT, IfcSIUnitName.METRE, null);
                    break;
                default:
                    throw new InvalidDataException($"Invalid lenght unit: {lenghtUnit}");
            }
        }

        private IfcBuildingStorey CreateStoreyHierarchy(IfcStore model, Options opts)
        {
            using (var txn = model.BeginTransaction("Create structure"))
            {
                var site = model.Instances.New<IfcSite>();
                site.Name = opts.SiteName ?? "Default Site";
                site.ObjectPlacement = MakePlacement(model, 0, 0, 0);

                var building = model.Instances.New<IfcBuilding>();
                building.Name = opts.BuildingName ?? "Default Building";
                building.CompositionType = IfcElementCompositionEnum.ELEMENT;
                building.ObjectPlacement = MakePlacement(model, 0, 0, 0);

                var storey = model.Instances.New<IfcBuildingStorey>();
                storey.Name = opts.StoreyName ?? "Default Storey";
                storey.ObjectPlacement = MakePlacement(model, 0, 0, 0);


                //get the project there should only be one and it should exist
                var project = model.Instances.OfType<IfcProject>().FirstOrDefault();

                project?.AddSite(site);
                site.AddBuilding(building);
                building.AddToSpatialDecomposition(storey);

                txn.Commit();
                return storey;
            }
        }

        private static IfcLocalPlacement MakePlacement(IfcStore model, double x, double y, double z)
        {
            var localPlacement = model.Instances.New<IfcLocalPlacement>();
            var placement = model.Instances.New<IfcAxis2Placement3D>();
            localPlacement.RelativePlacement = placement;
            placement.Location = model.Instances.New<IfcCartesianPoint>(p => p.SetXYZ(x, y, z));
            return localPlacement;
        }

        private IfcBuilding CreateBuilding(IfcStore model, Options opts)
        {
            using (var txn = model.BeginTransaction("Create Building"))
            {
                var building = model.Instances.New<IfcBuilding>();
                building.Name = opts.BuildingName ?? "Default Building";

                building.CompositionType = IfcElementCompositionEnum.ELEMENT;
                var localPlacement = model.Instances.New<IfcLocalPlacement>();
                building.ObjectPlacement = localPlacement;
                var placement = model.Instances.New<IfcAxis2Placement3D>();
                localPlacement.RelativePlacement = placement;
                placement.Location = model.Instances.New<IfcCartesianPoint>(p => p.SetXYZ(0, 0, 0));
                //get the project there should only be one and it should exist
                var project = model.Instances.OfType<IfcProject>().FirstOrDefault();
                project?.AddBuilding(building);
                txn.Commit();
                return building;
            }
        }
    }
}
