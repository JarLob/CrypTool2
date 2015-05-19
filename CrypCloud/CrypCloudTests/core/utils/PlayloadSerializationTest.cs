 using CrypCloud.Core;
 using CrypCloud.Core.CloudComponent;
 using CrypCloud.Core.utils;
 using Microsoft.VisualStudio.TestTools.UnitTesting;
 using WorkspaceManager.Model;

namespace CrypCloudTests.utils
{
    [TestClass]
    public class PlayloadSerializationTest
    {
        [TestMethod]
        public void Serialization_workspaceModel()
        {
            var workspaceModel = new WorkspaceModel();
            byte[] result = PayloadSerialization.Serialize(workspaceModel);

            Assert.IsNotNull(result);
            Assert.AreNotEqual(1, result.Length);
        }

        [TestMethod]
        public void Deserialization_workspaceModel()
        {
            var workspaceModel = new WorkspaceModel() {Zoom = 1337};
            var result = PayloadSerialization.Serialize(workspaceModel);

            var model = PayloadSerialization.Deserialize(result);

            Assert.AreEqual(model.Zoom, workspaceModel.Zoom);
        } 
    }
}