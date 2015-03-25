﻿using System.IO;
using WorkspaceManager.Model;

namespace CrypCloud.Core.utils
{
    public static class PayloadSerialization
    {
        public static byte[] Serialize(WorkspaceModel workspaceModel)
        {
            using (var stream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(stream))
                {
                    var persistantModel = new ModelPersistance().GetPersistantModel(workspaceModel);
                    XMLSerialization.XMLSerialization.Serialize(persistantModel, streamWriter);
                }
                return stream.ToArray();
            }
        } 

        public static WorkspaceModel Deserialize(byte[] jobPayload)
        {
            using (var stream = new MemoryStream())
            {
                stream.Write(jobPayload, 0, jobPayload.Length);
                using (var streamWriter = new StreamWriter(stream))
                {
                    return new ModelPersistance().loadModel(streamWriter);
                }
            }
        }
    }
}