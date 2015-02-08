using System;
using System.Text.RegularExpressions;

namespace PeersAtPlay.CertificateLibrary.Util
{
    public static class Verification
    {
        public static bool IsValidCommonName(string cn)
        {
            return !String.Empty.Equals(cn);
        }

        public static bool IsValidOrganisation(string o)
        {
            return ! String.Empty.Equals(o);
        }

        public static bool IsValidOrganisationalUnit(string ou)
        {
            return ! String.Empty.Equals(ou);
        }

        public static bool IsValidEmailAddress(string email)
        {
            if (email == null || String.Empty.Equals(email) || email.Length > 60)
            {
                return false;
            }

            var emailRegex = new Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
            return emailRegex.IsMatch(email);
        }

        public static bool IsValidPassword(string password)
        {
            return !String.Empty.Equals(password) && password.Length <= 40;
        }


        public static bool IsValidAvatar(string avatar)
        {
            return !String.Empty.Equals(avatar) && avatar.Length <= 40;
        }

        public static bool IsValidWorld(string world)
        {
            return !String.Empty.Equals(world) && world.Length <= 40;
        }

        public static bool IsValidCode(string code)
        {
            return !String.Empty.Equals(code) && code.Length <= 40;
        }
    }
}
