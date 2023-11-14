using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AutoCADAssignmentPlugin
{
    internal static class Helpers
    {
        internal static Entity CreateBoxAtPosition(Point3d position, Vector3d size)
        {
            //Create a box and set its size and position
            Solid3d boxSolid = new Solid3d();

            boxSolid.CreateBox(size.X, size.Y, size.Z);
            boxSolid.TransformBy(Matrix3d.Displacement(position.GetAsVector()));

            return boxSolid;
        }

        internal static void RotateBlockReference(string blockReferenceName, double angle, Vector3d rotationAxis, Database database)
        {
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord blockTableRecord = transaction.GetObject(blockTable[blockReferenceName], OpenMode.ForWrite) as BlockTableRecord;

                foreach (ObjectId objId in blockTableRecord)
                {
                    Entity entity = transaction.GetObject(objId, OpenMode.ForWrite) as Entity;

                    entity.TransformBy(Matrix3d.Rotation(angle, rotationAxis, entity.GeometricExtents.MinPoint));
                }

                transaction.Commit();
            }
        }
    }
}
