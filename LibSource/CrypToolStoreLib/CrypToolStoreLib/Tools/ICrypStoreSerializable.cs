using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypToolStoreLib.Tools
{
    /// <summary>
    /// Interface for serializable CrypToolStoreObjects
    /// </summary>
    public interface ICrypToolStoreSerializable
    {
        byte[] Serialize();
        void Deserialize(byte[] bytes);
    }
}
