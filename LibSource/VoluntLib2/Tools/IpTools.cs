/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Net;

namespace VoluntLib2.Tools
{
    public class IpTools
    {
        /// <summary>
        /// Set of private IP ranges for IsPrivateIP method
        /// </summary>
        private static uint[][] ipranges = new uint[][]
        {
            new uint[]{167772160u,   184549375u}, /*    10.0.0.0 -  10.255.255.255 */
            new uint[]{3232235520u, 3232301055u}, /* 192.168.0.0 - 192.168.255.255 */
            new uint[]{2130706432u, 2147483647u}, /*   127.0.0.0 - 127.255.255.255 */
            new uint[]{2851995648u, 2852061183u}, /* 169.254.0.0 - 169.254.255.255 */
            new uint[]{2886729728u, 2887778303u}, /*  172.16.0.0 -  172.31.255.255 */
            new uint[]{3758096384u, 4026531839u}, /*   224.0.0.0 - 239.255.255.255 */
            new uint[]{0u, 0u},                   /*     0.0.0.0 - 0.0.0.0.        */
            new uint[]{4294967295u, 4294967295u}, /*  255.255.255.255 - 255.255.255.255 */
        };

        /// <summary>
        /// Checks if a given IP adress is inside one of the private ip address ranges
        /// </summary>
        /// <param name="ipaddress"></param>
        /// <returns></returns>
        public static bool IsPrivateIP(IPAddress ipaddress)
        {
            byte[] bytes = ipaddress.GetAddressBytes();
            bytes = new byte[] { bytes[3], bytes[2], bytes[1], bytes[0] }; //reverse the ip for calculation
            uint ipNumber = BitConverter.ToUInt32(bytes, 0);
            foreach (uint[] range in ipranges)
            {
                if (ipNumber >= range[0] && ipNumber <= range[1])
                {
                    return true;
                }
            }
            return false;
        }
    }
}
