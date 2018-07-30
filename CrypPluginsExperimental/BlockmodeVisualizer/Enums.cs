using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * Useful enums for the BlockmodeVisualizer
 */
namespace BlockmodeVisualizer
{
    public enum Actions { ENCRYPTION, DECRYPTION }
    public enum Blockmodes { ECB, CBC, CFB, OFB, CTR, XTS, CCM, GCM }
}
