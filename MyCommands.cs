using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(AutoCADAssignmentPlugin.MyCommands))]
namespace AutoCADAssignmentPlugin
{
    public class MyCommands
    {
        private readonly static string _boxName = "Boxes";
        private static int _objCount = 0;

        [CommandMethod("CreateBoxes")]
        public static void CreateBoxes()
        {
            Document document = Application.DocumentManager.MdiActiveDocument;
            Database database = document.Database;

            PromptPointResult promptResultPoint = document.Editor.GetPoint("\nWhere?");
            if (promptResultPoint.Status != PromptStatus.OK) return;

            var point = promptResultPoint.Value;
            string boxName = $"{_boxName}_{_objCount}";

            //Group in a block reference
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord blockTableRecord = new BlockTableRecord();
                blockTableRecord.Name = boxName;

                blockTableRecord.AppendEntity(Helpers.CreateBoxAtPosition(new Point3d(0, 0, 0), new Vector3d(1, 1, 1)));
                blockTableRecord.AppendEntity(Helpers.CreateBoxAtPosition(new Point3d(5, 0, 0), new Vector3d(1, 1, 1)));
                blockTableRecord.AppendEntity(Helpers.CreateBoxAtPosition(new Point3d(10, 0, 0), new Vector3d(1, 1, 1)));

                //Add block table record to database
                transaction.GetObject(database.BlockTableId, OpenMode.ForWrite);
                blockTable.Add(blockTableRecord);
                transaction.AddNewlyCreatedDBObject(blockTableRecord, true);

                using (BlockReference blockRef = new BlockReference(point, blockTable[boxName]))
                {
                    BlockTableRecord space = transaction.GetObject(database.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                    space.AppendEntity(blockRef);
                    transaction.AddNewlyCreatedDBObject(blockRef, true);
                }

                transaction.Commit();
            }

            //Rotate the block reference around X, Y, and Z axes
            PromptDoubleResult promptResultXAngle = document.Editor.GetAngle("\nX Angle?");
            if (promptResultXAngle.Status != PromptStatus.OK) return;
            Helpers.RotateBlockReference(boxName, promptResultXAngle.Value, Vector3d.XAxis, database);

            PromptDoubleResult promptResultYAngle = document.Editor.GetAngle("\nY Angle?");
            if (promptResultYAngle.Status != PromptStatus.OK) return;
            Helpers.RotateBlockReference(boxName, promptResultYAngle.Value, Vector3d.YAxis, database);

            PromptDoubleResult promptResultZAngle = document.Editor.GetAngle("\nZ Angle?");
            if (promptResultZAngle.Status != PromptStatus.OK) return;
            Helpers.RotateBlockReference(boxName, promptResultZAngle.Value, Vector3d.ZAxis, database);

            _objCount++;

            // Refresh the display
            document.Editor.Regen();
        }
    }
}