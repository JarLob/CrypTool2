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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.Enigma
{
    internal partial class EnigmaCore
    {
        private const string A3 = "ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const int alen = 26; // Alphabet Length

        private readonly string[,] rotors = new string[,]{
                                        { // Kommerzielle Enigma A/B, ab 1924
                                            "DMTWSILRUYQNKFEJCAZBPGXOHV",
                                            "HQZGPJTMOBLNCIFDYAWVEUSRXL",
                                            "UQNTLSZFMREHDPLKIBVYGJCWGA",
                                            "",
                                            "",
                                            "",
                                            "",
                                            "",
                                        },
                                        { // Kommerzielle Enigma D
                                          // ETW: JWULCMNOHPQZYXIRADKEGVBTSF
                                            "LPGSZMHAEOQKVXRFYBUTNICJDW",
                                            "SLVGBTFXJQOHEWIRZYAMKPCNDU",
                                            "CJGDPSHKTURAWZXFMYNQOBVLIE",
                                            "",
                                            "",
                                            "",
                                            "",
                                            "",
                                        },
                                        { // Enigma der Reichsbahn („Rocket“), ab 7. Feb 1941
                                          // ETW: QWERTZUIOASDFGHJKPYXCVBNML
                                            "JGDQOXUSCAMIFRVTPNEWKBLZYH",
                                            "NTZPSFBOKMWRCJDIVLAEYUXHGQ",
                                            "JVIUBHTCDYAKEQZPOSGXNRMWFL",
                                            "",
                                            "",
                                            "",
                                            "",
                                            "",
                                        },
                                        {   // Enigma I, ab 1930, Walzen IV ab 1938, Walzen V-VII ab 1938
                                            // ETW: ABCDEFGHIJKLMNOPQRSTUVWXYZ
                                            "EKMFLGDQVZNTOWYHXUSPAIBRCJ", // I
                                            "AJDKSIRUXBLHWTMCQGZNPYFVOE", // II
                                            "BDFHJLCPRTXVZNYEIWGAKMUSQO", // III
                                            "ESOVPZJAYQUIRHXLNFTGKDCMWB", // IV
                                            "VZBRGITYUPSDNHLXAWMJQOFECK", // V
                                            "JPGVOUMFYQBENHZRDKASXLICTW", // VI
                                            "NZJHGRCXMYSWBOUFAIVLPEKQDT", // VII
                                            "FKQHTLXOCBJSPDZRAMEWNIUYGV"  // VIII
                                        },
                                        {   // Enigma M4 "Shark"
                                            // ETW: ABCDEFGHIJKLMNOPQRSTUVWXYZ
                                            "LEYJVCNIXWPBQMDRTAKZGFUHOS", // Beta, ab 1 Feb. 1942
                                            "FSOKANUERHMBTIYCWLQPZXVGJD", // Gamma, ab 1. Juli 1943
                                            "",
                                            "",
                                            "",
                                            "",
                                            "",
                                            "",
                                        }
                                  };
        private readonly string[,] reflectors = {
                                           {  // Kommerzielle Enigma A/B - there was no reflector
                                              "", 
                                              "", 
                                              ""  
                                           },
                                           {  // Kommerzielle Enigma D
                                              "IMETCGFRAYSQBZXWLHKDVUPOJN", 
                                              "", 
                                              ""  
                                           },
                                           {  // Enigma der Reichsbahn („Rocket“), ab 7. Feb 1941
                                              "QYHOGNECVPUZTFDJAXWMKISRBL", 
                                              "", 
                                              ""  
                                           },
                                           {  // Enigma I, ab 1930, Walzen IV ab 1938, Walzen V-VII ab 1938
                                              "EJMZALYXVBWFCRQUONTSPIKHGD", // UKW A
                                              "YRUHQSLDPXNGOKMIEBFZCWVJAT", // UKW B
                                              "FVPJIAOYEDRZXWGCTKUQSBNMHL"  // UKW C
                                           },
                                           {  // Enigma M4 "Shark"
                                              "", 
                                              "ENKQAUYWJICOPBLMDXZVFTHRGS", 
                                              "RDOBJNTKVEHMLFCWZAXGYIPSUQ"  
                                           },
                                       };

        private readonly string[] notches = {
                                       "Q",  // I
                                       "E",  // II
                                       "V",  // III
                                       "J",  // IV
                                       "Z",  // V
                                       "ZM", // VI
                                       "ZM", // VII
                                       "ZM"  // VIII
                                    };


        

    }
}
