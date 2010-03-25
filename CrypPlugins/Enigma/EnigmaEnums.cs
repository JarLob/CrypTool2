/*
   Copyright 2008 Dr. Arno Wacker, University of Duisburg-Essen

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

namespace Cryptool.Enigma
{

    enum rotorEnum { I = 0, II = 1, III = 2, IV = 3, V = 4, VI = 5, VII = 6, VIII = 7 };
    enum rotor4Enum { Beta = 0, Gamma = 1 };
    enum reflectorEnum { A = 0, B = 1, C = 2 };
    enum enigmaModelEnum { EnigmaAB = 0, EnigmaD = 1, Rocket = 2, EnigmaM3 = 3, EnigmaM4 = 4, EnigmaK = 5, EnigmaG = 6 };

    enum VerboseLevels { VeryVerbose, Verbose, Normal, Quiet }

}
