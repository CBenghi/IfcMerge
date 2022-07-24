using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc4.Interfaces;

namespace IfcMerge
{
    internal class UnifiedEntity : IEquatable<UnifiedEntity>
    {
        public string type;
        public string name;
        

        public UnifiedEntity(IIfcRoot namedSpace)
        {
            this.type = namedSpace.GetType().Name;
            this.name = namedSpace.Name.ToString() ?? "";
        }

        public UnifiedEntity(IIfcRepresentationContext context)
        {
            // todo: the context should be more precisely identified by the location, otherwise there's a risk of misplaced parts.
            switch (context)
            {
                case IIfcGeometricRepresentationSubContext ctx:
                    this.type = context.GetType().Name;
                    this.name = $"{ctx.ContextIdentifier}_{ctx.ContextType}_{ctx.Precision}_{Serialize(ctx.WorldCoordinateSystem)}";
                    break;
                case IIfcGeometricRepresentationContext sub:
                    this.type = context.GetType().Name;
                    this.name = $"{sub.ContextIdentifier}_{sub.ContextType}_{sub.Precision}_{Serialize(sub.WorldCoordinateSystem)}";
                    break;
                default:
                    this.type = context.GetType().Name;
                    this.name = $"{context.ContextIdentifier}_{context.ContextType}";
                    break;
            }
        }

        private string Serialize(IIfcAxis2Placement coordinateSystem)
        {
            if (coordinateSystem is IIfcAxis2Placement2D case2D)
            {
                var coords = case2D.Location is not null ? string.Join(",", case2D.Location.Coordinates.Select(coor => coor.Value).ToArray()) : "";
                var refd = case2D.RefDirection is not null ? string.Join(",", case2D.RefDirection.DirectionRatios.Select(coor => coor.Value).ToArray()) : "";
                return $"{coords}_{refd}";
            }
            if (coordinateSystem is IIfcAxis2Placement3D case3D)
            {
                var coords = case3D.Location is not null ? string.Join(",", case3D.Location.Coordinates.Select(coor => coor.Value).ToArray()) : "";
                var axis = case3D.Axis is not null ? string.Join(",", case3D.Axis.DirectionRatios.Select(coor => coor.Value).ToArray()) : "";
                var refd = case3D.RefDirection is not null ? string.Join(",", case3D.RefDirection.DirectionRatios.Select(coor => coor.Value).ToArray()) : "";
                return $"{coords}_{axis}_{refd}";
            }
            return "";
        }

        public UnifiedEntity(string type, string name)
        {
            this.type = type;
            this.name = name;
        }

        public bool Equals(UnifiedEntity? other)
        {
            if (other is null) 
                return false;
            return this.type == other.type && this.name == other.name;
        }

        public override bool Equals(object other)
        {
            return Equals(other as UnifiedEntity);
        }

        public override int GetHashCode()
        {
            return (name, type).GetHashCode();
        }
    }
}
