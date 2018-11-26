package enigma;

import common.*;

public class Main {
    private enum Mode {
        HILLCLIMBING,
        IC,
        TRIGRAMS,
        BOMBE,
        INDICATORS,
        INDICATORS1938,
        SCENARIO,
        DECRYPT
    }

    private static void incompatible(Mode mode, Flag[] flags) {
        for (Flag flag : flags) {
            if (CommandLine.isSet(flag)) {
                CtAPI.goodbyeError("Flag -%s (%s) is incompatible with mode %s\n", CommandLine.getFlagString(flag), CommandLine.getShortDesc(flag), mode);
            }
        }
    }

    private static void required(Mode mode, Key.Model currentModel, Key.Model[] models) {
        for (Key.Model model : models) {
            if (model == currentModel) {
                return;
            }
        }
        CtAPI.goodbyeError("Mode %s is incompatible with model %s\n", mode, currentModel);
    }

    private static void required(Mode mode, Flag[] flags) {
        for (Flag flag : flags) {
            if (!CommandLine.isSet(flag)) {
                CtAPI.goodbyeError("Flag -%s (%s) is mandatory with mode %s\n", CommandLine.getFlagString(flag), CommandLine.getShortDesc(flag), mode);
            }
        }
    }

    private static void incompatibleWithRangeOkKeys(Flag[] flags) {
        for (Flag flag : flags) {
            if (CommandLine.isSet(flag)) {
                CtAPI.goodbyeError("Flag -%s (%s) is not allowed with a key range\n", CommandLine.getFlagString(flag), CommandLine.getShortDesc(flag));
            }
        }
    }

    private static void incompatibleWithSingleKey(Flag[] flags) {
        for (Flag flag : flags) {
            if (CommandLine.isSet(flag)) {
                CtAPI.goodbyeError("Flag -%s (%s) requires a key range\n", CommandLine.getFlagString(flag), CommandLine.getShortDesc(flag));
            }
        }
    }


    private static void incompatibleWithRangeOkKeys(Mode currentMode, Mode[] modes) {
        for (Mode mode : modes) {
            if (mode == currentMode) {
                CtAPI.goodbyeError("Mode %s is not allowed with a key range\n", mode);
            }
        }

    }

    private static void incompatibleWithSingleKey(Mode currentMode, Mode[] modes) {
        for (Mode mode : modes) {
            if (mode == currentMode) {
                CtAPI.goodbyeError("Mode %s requires a key range\n", mode);
            }
        }
    }


    public static void main(String[] args) {

        createCommandLineArguments();
        //CommandLineArgument.printUsage();
        BestResults.setDiscardSamePlaintexts(false);
        BestResults.setThrottle(false);
        BestResults.setMaxNumberOfResults(100);

        CtAPI.open("Enigma attacks", "1.0");

        String[] ctArgs = CtAPI.getArgs();
        if (!CommandLine.parseArguments(ctArgs, false)) {
            CommandLine.printUsage();
            return;
        }

        if (!CommandLine.parseArguments(args, true)) {
            CommandLine.printUsage();
            return;
        }

        CommandLine.printArguments();

        final String RESOURCE_PATH = CommandLine.getStringValue(Flag.RESOURCE_PATH);
        final int HILLCLIMBING_CYCLES = CommandLine.getIntegerValue(Flag.HILLCLIMBING_CYCLES);
        final int THREADS = CommandLine.getIntegerValue(Flag.THREADS);
        final String CRIB = CommandLine.getStringValue(Flag.CRIB);
        String CIPHERTEXT = CommandLine.getStringValue(Flag.CIPHERTEXT);
        if (CIPHERTEXT.endsWith("txt")) {
            CIPHERTEXT = common.Utils.readTextFile(CIPHERTEXT).toUpperCase().replaceAll("[^A-Z]", "");
        }

        final Key.Model MODEL = Key.Model.valueOf(CommandLine.getStringValue(Flag.MODEL));
        final Language LANGUAGE = Language.valueOf(CommandLine.getStringValue(Flag.LANGUAGE));
        final String INDICATORS_FILE = CommandLine.getStringValue(Flag.INDICATORS_FILE);
        final String KEY = CommandLine.getStringValue(Flag.KEY);
        final String MESSAGE_INDICATOR = CommandLine.getStringValue(Flag.MESSAGE_INDICATOR);
        final String SCENARIO = CommandLine.getStringValue(Flag.SCENARIO);

        final int RIGHT_ROTOR_SAMPLING = CommandLine.getIntegerValue(Flag.RIGHT_ROTOR_SAMPLING);
        final MRingScope MIDDLE_RING_SCOPE = MRingScope.valueOf(CommandLine.getIntegerValue(Flag.MIDDLE_RING_SCOPE));
        final boolean VERBOSE = CommandLine.getBooleanValue(Flag.VERBOSE);
        final String CRIB_POSITION = CommandLine.getStringValue(Flag.CRIB_POSITION);
        final Mode MODE = Mode.valueOf(CommandLine.getStringValue(Flag.MODE));

        String[] keyAndStecker = KEY.split("[|]");
        String steckerS = (keyAndStecker.length == 2) ? keyAndStecker[1] : "";
        String[] keyParts = keyAndStecker[0].split("[\\-]");
        boolean range = keyParts.length == 2;
        String rangeLowS = keyParts[0];
        String rangeHighS = range ? keyParts[1] : "";
        ;
        String keyS = range ? "" : rangeLowS;

        String indicatorS = "";
        String indicatorMessageKeyS = "";

        switch (MODE) {
            case HILLCLIMBING:
                required(MODE, new Flag[]{Flag.CIPHERTEXT});
                incompatible(MODE, new Flag[]{Flag.SCENARIO, Flag.CRIB, Flag.CRIB_POSITION, Flag.INDICATORS_FILE});
                break;
            case IC:
                required(MODE, new Flag[]{Flag.CIPHERTEXT});
                incompatible(MODE, new Flag[]{Flag.SCENARIO, Flag.CRIB, Flag.CRIB_POSITION, Flag.INDICATORS_FILE});
                break;
            case TRIGRAMS:
                required(MODE, new Flag[]{Flag.CIPHERTEXT});
                incompatible(MODE, new Flag[]{Flag.SCENARIO, Flag.CRIB, Flag.CRIB_POSITION, Flag.INDICATORS_FILE});
                break;
            case BOMBE:
                required(MODE, new Flag[]{Flag.CIPHERTEXT, Flag.CRIB});
                incompatible(MODE, new Flag[]{Flag.SCENARIO, Flag.INDICATORS_FILE});
                break;
            case INDICATORS:
                required(MODE, new Flag[]{Flag.INDICATORS_FILE});
                incompatible(MODE, new Flag[]{Flag.SCENARIO, Flag.CRIB, Flag.CRIB_POSITION, Flag.MESSAGE_INDICATOR});
                required(MODE, MODEL, new Key.Model[]{Key.Model.H});
                break;
            case INDICATORS1938:
                required(MODE, new Flag[]{Flag.INDICATORS_FILE});
                incompatible(MODE, new Flag[]{Flag.SCENARIO, Flag.CRIB, Flag.CRIB_POSITION, Flag.MESSAGE_INDICATOR, Flag.MIDDLE_RING_SCOPE, Flag.RIGHT_ROTOR_SAMPLING});
                required(MODE, MODEL, new Key.Model[]{Key.Model.H});
                break;
            case SCENARIO:
                required(MODE, new Flag[]{Flag.SCENARIO});
                incompatible(MODE, new Flag[]{Flag.LANGUAGE, Flag.HILLCLIMBING_CYCLES, Flag.CRIB, Flag.CRIB_POSITION, Flag.INDICATORS_FILE, Flag.MESSAGE_INDICATOR, Flag.MIDDLE_RING_SCOPE, Flag.RIGHT_ROTOR_SAMPLING});
                break;
            case DECRYPT:
                incompatible(MODE, new Flag[]{Flag.CRIB, Flag.CRIB_POSITION, Flag.INDICATORS_FILE, Flag.MESSAGE_INDICATOR, Flag.MIDDLE_RING_SCOPE, Flag.RIGHT_ROTOR_SAMPLING});
                break;
        }


        if (range) {
            incompatibleWithRangeOkKeys(MODE, new Mode[]{Mode.DECRYPT});
            incompatibleWithRangeOkKeys(new Flag[]{});
        } else {
            incompatibleWithSingleKey(new Flag[]{Flag.MIDDLE_RING_SCOPE, Flag.RIGHT_ROTOR_SAMPLING});
            incompatibleWithSingleKey(MODE, new Mode[]{Mode.IC, Mode.TRIGRAMS, Mode.INDICATORS, Mode.INDICATORS1938});
        }


        if (!MESSAGE_INDICATOR.isEmpty()) {
            Key dummyKey = new Key();
            boolean indicatorError = false;
            switch (MESSAGE_INDICATOR.length()) {
                case 3:
                    dummyKey.initDefaults(Key.Model.H);
                    indicatorS = MESSAGE_INDICATOR;
                    if (dummyKey.setMesg(indicatorS) != 1)
                        indicatorError = true;
                    break;
                case 4:
                    dummyKey.initDefaults(Key.Model.M4);
                    indicatorS = MESSAGE_INDICATOR;
                    if (dummyKey.setMesg(indicatorS) != 1)
                        indicatorError = true;
                    break;
                case 7:
                    dummyKey.initDefaults(Key.Model.H);
                    indicatorMessageKeyS = MESSAGE_INDICATOR.substring(0, 3);
                    indicatorS = MESSAGE_INDICATOR.substring(4, 7);
                    if (dummyKey.setMesg(indicatorS) != 1)
                        indicatorError = true;
                    if (dummyKey.setMesg(indicatorMessageKeyS) != 1)
                        indicatorError = true;
                    break;
                case 9:
                    dummyKey.initDefaults(Key.Model.M4);
                    indicatorMessageKeyS = MESSAGE_INDICATOR.substring(0, 4);
                    indicatorS = MESSAGE_INDICATOR.substring(5, 9);
                    if (dummyKey.setMesg(indicatorS) != 1)
                        indicatorError = true;
                    if (dummyKey.setMesg(indicatorMessageKeyS) != 1)
                        indicatorError = true;
                    break;

                default:
                    indicatorError = true;
                    break;
            }

            if (indicatorError) {
                CtAPI.goodbyeError("Invalid Indicator (-%s): Either XXX or XXX:YYY for Model H/M3, or XXXX:YYYY for Model M4\n", CommandLine.getFlagString(Flag.MESSAGE_INDICATOR));
            } else if (indicatorMessageKeyS.length() == 0) { // xxx format
                if (range) {
                    CtAPI.goodbyeError("Invalid Indicator: When range of keys selected, then only -%s XXX:YYY or XXXX:YYYY (for M4) is allowed \n", CommandLine.getFlagString(Flag.MESSAGE_INDICATOR));
                }
            } else {// xxx:yyy format
                if (!range) {
                    CtAPI.goodbyeError("Invalid Indicator (-w): If single key selected, then only -%s XXX (or XXXX for M4) is allowed \n", CommandLine.getFlagString(Flag.MESSAGE_INDICATOR));
                }
            }
        }

        int res;

        byte plaintext[] = new byte[Key.MAXLEN];
        byte ciphertext[] = new byte[Key.MAXLEN];
        int clen = 0;
        Key lowKey = new Key();
        Key highKey = new Key();
        Key key = new Key();

        String bigramFile = "enigma_logbigrams.txt";
        String trigramFile = "enigma_logtrigrams.txt";
        if (LANGUAGE == Language.ENGLISH) {
            bigramFile = "english_logbigrams.txt";
            trigramFile = "english_logtrigrams.txt";
        }
        bigramFile = RESOURCE_PATH +"\\" +bigramFile;
        trigramFile = RESOURCE_PATH +"\\" +trigramFile;
        res = Stats.loadTridict(trigramFile);
        if (res != 1) {
            CtAPI.goodbyeError("Load (log) trigrams file %s failed\n", trigramFile);
        }
        res = Stats.loadBidict(bigramFile);
        if (res != 1) {
            CtAPI.goodbyeError("Load (log) bigrams file %s failed\n", bigramFile);
        }

        if (!CommandLine.isSet(Flag.SCENARIO)) {
            clen = Utils.getText(CIPHERTEXT, ciphertext);
            CtAPI.printf("Ciphertext (Length = %d) %s\n", clen, CIPHERTEXT);
        }

        if (!range) {
            res = key.setKey(keyS, MODEL, false);
            if (res != 1) {
                CtAPI.goodbyeError("Invalid key: %s\n", keyS);
            }

            res = key.setStecker(steckerS);
            if (res != 1) {
                CtAPI.goodbyeError("invalid stecker board settings: %s - Should include pairs of letters with no repetitions, or may be omitted\n", steckerS);
            }

            if (indicatorS.length() != 0) {
                Key dumpKey = new Key(key);
                res = dumpKey.setMesg(indicatorS);
                if (res == 0) {
                    CtAPI.goodbyeError("Invalid message message_indicator: %s \n", indicatorS);
                }
                if (steckerS.length() == 0) {
                    CtAPI.goodbyeError("Stecker board mandatory when -%s is specified\n", CommandLine.getFlagString(Flag.MESSAGE_INDICATOR));
                }
            }

        } else {

            String fminS;
            String tmaxS;
            switch (MODEL) {
                case M3:
                    fminS = "B:111:AAA:AAA";
                    tmaxS = "C:888:ZZZ:ZZZ";
                    break;
                case M4:
                    fminS = "B:B111:AAAA:AAAA";
                    tmaxS = "C:G888:ZZZ:ZZZZ";

                    break;
                case H:
                default:
                    fminS = "A:111:AAA:AAA";
                    tmaxS = "C:555:ZZZ:ZZZ";
                    break;
            }

            if (rangeLowS.length() == 0)
                rangeLowS = fminS;

            if (rangeHighS.length() == 0)
                rangeHighS = tmaxS;


            res = Key.setRange(lowKey, highKey, rangeLowS, rangeHighS, MODEL);
            if (res != 1) {
                CtAPI.goodbyeError("Invalid key range:  %s-%s  - Invalid key format, or first has higher value than last \n", rangeLowS, rangeHighS);
            }

            if ((lowKey.lRing != highKey.lRing) && (indicatorS.length() == 0) && (MODE == Mode.HILLCLIMBING))
                System.out.print("\n\n\nWARNING: Setting a range (different values) for the Left Ring settings is usually not necessary and will significant slow Hill Climbing searche\n\n\n");

            if (steckerS.length() != 0) {
                res = lowKey.setStecker(steckerS) * highKey.setStecker(steckerS);
                if (res != 1) {
                    CtAPI.goodbyeError("Invalid steckers: %s - Should include pairs of letters with no repetitions, or be omitted\n", steckerS);
                }
            }


            if ((indicatorS.length() != 0) || (indicatorMessageKeyS.length() != 0)) {


                if ((indicatorS.length() == 0) || (indicatorMessageKeyS.length() == 0)) {
                    CtAPI.goodbyeError("Invalid message_indicator (-%s) - Only XXX:YYY (or XXXX:YYYY for M4), which must include the Message Key for encrypting the Indicator, is allowed for this mode %s\n",
                            CommandLine.getFlagString(Flag.MESSAGE_INDICATOR), MODE);
                }
                Key tempKey = new Key(lowKey);
                res = tempKey.setMesg(indicatorS);
                if (res == 0) {
                    CtAPI.goodbyeError("Invalid message indicator (-%s): %s \n", CommandLine.getFlagString(Flag.MESSAGE_INDICATOR), indicatorS);
                }
                res = tempKey.setMesg(indicatorMessageKeyS);
                if (res == 0) {
                    CtAPI.goodbyeError("Invalid message key for message_indicator (-%s): %s \n", CommandLine.getFlagString(Flag.MESSAGE_INDICATOR), indicatorMessageKeyS);
                }
                if (steckerS.length() == 0) {
                    CtAPI.goodbyeError("Stecker board settings mandatory for -%s  \n", CommandLine.getFlagString(Flag.MESSAGE_INDICATOR));
                }
                if (HILLCLIMBING_CYCLES > 0) {
                    CtAPI.goodbyeError("Invalid settings - When specifying -%s , -%s 0 (no Hill Climbing on search results) must also be selected. \n",
                            CommandLine.getFlagString(Flag.MESSAGE_INDICATOR), CommandLine.getFlagString(Flag.HILLCLIMBING_CYCLES));
                }
            }

            if (MIDDLE_RING_SCOPE != MRingScope.ALL) {
                boolean sameLowHighMRing = (lowKey.mRing == highKey.mRing);
                boolean fullRangeMRing = (lowKey.mRing == Utils.getIndex('A')) &&
                        (highKey.mRing == Utils.getIndex('Z'));
                if (!sameLowHighMRing && !fullRangeMRing) {
                    CtAPI.goodbyeError("Range of middle ring (%s-%s) imcompatible with -%s selection: Only -%s 0 (or not specifying -%s and leaving the default 0) is allowed \n" +
                                    " when a specifying a partial range. Either use A to Z, or same value for low Middle Ring and high Middle Ring, or use -%s 0.\n",
                            Utils.getChar(lowKey.mRing),
                            Utils.getChar(highKey.mRing),
                            CommandLine.getFlagString(Flag.MIDDLE_RING_SCOPE),
                            CommandLine.getFlagString(Flag.MIDDLE_RING_SCOPE),
                            CommandLine.getFlagString(Flag.MIDDLE_RING_SCOPE),
                            CommandLine.getFlagString(Flag.MIDDLE_RING_SCOPE)
                    );
                }
                if (clen > 400) {
                    CtAPI.goodbyeError("Message too long for -%s selection - Length is %d, -%s allowed only for messages shorter than 400\n",
                            CommandLine.getFlagString(Flag.MIDDLE_RING_SCOPE), clen, CommandLine.getFlagString(Flag.MIDDLE_RING_SCOPE));
                }
            }


            if (RIGHT_ROTOR_SAMPLING != 1) {

                boolean sameLowHighRRing = (lowKey.rRing == highKey.rRing);
                boolean fullRangeRRing = (lowKey.rRing == Utils.getIndex('A')) && (highKey.rRing == Utils.getIndex('Z'));
                if (!sameLowHighRRing && !fullRangeRRing) {
                    CtAPI.goodbyeError("Right ring range (%s to %s) imcompatible with -%s %d: Only -%s 1 (or not specifying -%s and leaving the default 1) is allowed \n" +
                                    " when a specifying a partial range. Either use A to Z, or same value for low Middle Ring and high Middle Ring, or use -%s 1.\n",
                            Utils.getChar(lowKey.rRing),
                            Utils.getChar(highKey.rRing),
                            CommandLine.getFlagString(Flag.RIGHT_ROTOR_SAMPLING),
                            RIGHT_ROTOR_SAMPLING,
                            CommandLine.getFlagString(Flag.MIDDLE_RING_SCOPE),
                            CommandLine.getFlagString(Flag.RIGHT_ROTOR_SAMPLING),
                            CommandLine.getFlagString(Flag.MIDDLE_RING_SCOPE)
                    );
                }

            }

        }


        if (MODE == Mode.BOMBE) {
            bombeSearch(HILLCLIMBING_CYCLES, CRIB, RIGHT_ROTOR_SAMPLING, MIDDLE_RING_SCOPE, VERBOSE, CRIB_POSITION, MODE, range, THREADS, indicatorS, indicatorMessageKeyS, ciphertext, clen, lowKey, highKey, key);
        } else if (MODE == Mode.DECRYPT && !range) {
            encryptDecrypt(indicatorS, plaintext, ciphertext, clen, key);
        } else if (MODE == Mode.IC) {
            Search.searchTrigramIC(lowKey, highKey, true, MIDDLE_RING_SCOPE, RIGHT_ROTOR_SAMPLING, false, HILLCLIMBING_CYCLES, 0, THREADS, ciphertext, clen, indicatorS, indicatorMessageKeyS);
        } else if (MODE == Mode.TRIGRAMS) {
            Search.searchTrigramIC(lowKey, highKey, false, MIDDLE_RING_SCOPE, RIGHT_ROTOR_SAMPLING, false, HILLCLIMBING_CYCLES, 0, THREADS, ciphertext, clen, indicatorS, indicatorMessageKeyS);
        } else if (MODE == Mode.HILLCLIMBING) {
            HillClimb.hillClimbRange(range ? lowKey : key, range ? highKey : key, HILLCLIMBING_CYCLES, THREADS, 0, MIDDLE_RING_SCOPE, RIGHT_ROTOR_SAMPLING, ciphertext, clen);
        } else if (MODE == Mode.SCENARIO) {
            new RandomChallenges("faust.txt", lowKey, highKey, SCENARIO);
        } else if (MODE == Mode.INDICATORS) { // cycles
            indicatorsSearch(INDICATORS_FILE, RIGHT_ROTOR_SAMPLING, MIDDLE_RING_SCOPE, steckerS, HILLCLIMBING_CYCLES, THREADS, ciphertext, clen, lowKey, highKey);
        } else if (MODE == Mode.INDICATORS1938) {
            indicators1938Search(INDICATORS_FILE, steckerS, plaintext, ciphertext, clen, lowKey, highKey);
        }

        CtAPI.goodbye(0, "Ending ....");
        java.awt.Toolkit.getDefaultToolkit().beep();

    }

    private static void indicators1938Search(String INDICATORS_FILE, String steckerS, byte[] plaintext, byte[] ciphertext, int clen, Key lowKey, Key highKey) {
        Key key;
        int A = Utils.getIndex('A');
        int Z = Utils.getIndex('Z');
        if ((lowKey.lMesg != A) || (lowKey.mMesg != A) || (lowKey.rMesg != A) ||
                (highKey.lMesg != Z) || (highKey.mMesg != Z) || (highKey.rMesg != Z)) {
            System.out.print("WARNING: Z. Sheets Search (-D): Ignoring Message Key settings. \n\n");
        }

        if (steckerS.length() != 0) {
            System.out.print("WARNING: Z. Sheets Search (-D): Ignoring Stecker Settings. \n\n");
            lowKey.setStecker("");
            highKey.setStecker("");
        }

        byte indicData[] = new byte[Key.MAXLEN];
        int flen = -1;
        if (INDICATORS_FILE.length() != 0)
            flen = Utils.loadCipherText(INDICATORS_FILE, indicData, false);
        if ((flen < 9) || (flen % 9 != 0)) {
            CtAPI.goodbyeError("Z. Sheets Search (-%s INDICATORS1938): Failed to load indicators data from file %s (%d characters found).\n",
                    CommandLine.getFlagString(Flag.MODE), INDICATORS_FILE, flen);
        }
        CtAPI.printf("Zygalski Sheets Search: Read database - File %s Indicators %d \nFirst Indicator: %s\n",
                INDICATORS_FILE, flen / 9, Utils.getString(indicData, 9));

        key = Search.searchZSheets(indicData, flen, lowKey, highKey, ciphertext, clen);

        if (key == null) {
            CtAPI.print("\nZ. Sheets Search: No match found. \n");
            CtAPI.goodbyeError("Zygalski Sheets search failed");
        }
    }

    private static void indicatorsSearch(String INDICATORS_FILE, int RIGHT_ROTOR_SAMPLING, MRingScope MIDDLE_RING_SCOPE, String steckerS, int HILLCLIMBING_CYCLES, int THREADS, byte[] ciphertext, int clen, Key lowKey, Key highKey) {
        Key key;
        int A = Utils.getIndex('A');
        int Z = Utils.getIndex('Z');
            /*
            if ((lowKey.l_ring != Z)|| (lowKey.m_ring != Z) || (lowKey.r_ring != Z) ||
                (highKey.l_ring != Z) || (highKey.m_ring != Z) || (highKey.r_ring != Z)) {
                CtAPI.printf("WARNING: Cycle Match Search (-O): Ignoring range ring settings. Will use ZZZ instead.\n\n");
//                lowKey.l_ring = lowKey.m_ring = lowKey.r_ring = Z;
//                highKey.l_ring = highKey.m_ring = highKey.r_ring = Z;
                lowKey.l_ring = lowKey.m_ring = lowKey.r_ring = A;
                highKey.r_ring = Z;
                highKey.l_ring = highKey.m_ring = A;
            }
            */
        if (steckerS.length() != 0) {
            System.out.print("INDICATORS WARNING: Cycle Match Search (-O): Ignoring Stecker Settings. \n\n");
            lowKey.setStecker("");
            highKey.setStecker("");
        }

        byte indicData[] = new byte[Key.MAXLEN];
        int flen = -1;
        if (INDICATORS_FILE.length() != 0)
            flen = Utils.loadCipherText(INDICATORS_FILE, indicData, false);
        if ((flen < 6) || (flen % 6 != 0)) {
            CtAPI.goodbyeError("INDICATORS Cycle Match Search (-%s INDICATORS): Failed to load indicators data from file %s (%d characters found).\n",
                    CommandLine.getFlagString(Flag.MODE), INDICATORS_FILE, flen);
        }
        CtAPI.printf("INDICATORS Cycle Match Search: Read database file %s, %d Indicator groups found\nFirst Indicator: %s\n",
                INDICATORS_FILE, flen / 6, Utils.getString(indicData, 6));

        key = Search.searchCycleMatch(indicData, flen, lowKey, highKey, HILLCLIMBING_CYCLES, THREADS, ciphertext, clen, MIDDLE_RING_SCOPE, RIGHT_ROTOR_SAMPLING);
        if (key == null) {
            CtAPI.printf("\nINDICATORS Cycle Match Search: No match found. \n");
        }


    }

    private static void encryptDecrypt(String indicatorS, byte[] plaintext, byte[] ciphertext, int clen, Key key) {
        boolean encrypted = Utils.isTextEncrypted(key, ciphertext, clen, indicatorS);

        if (encrypted) {
            Key finalKey = new Key(key);
            if (indicatorS.length() != 0) {
                byte indicCrypt[] = new byte[3];
                byte indicPlain[] = new byte[3];
                String indicPlainS;
                String indicCryptS = indicatorS.toUpperCase();

                CtAPI.printf("Encrypted Indicator: \t%s\n\n", indicCryptS);
                key.printKeyString("Key for Indicator:\t");
                int ilen = Utils.getText(indicCryptS, indicCrypt);
                key.encipherDecipherAll(indicCrypt, indicPlain, ilen);
                indicPlainS = Utils.getString(indicPlain, ilen);
                CtAPI.printf("\nPlain Indicator: %s\n\n", indicPlainS);

                finalKey.setMesg(indicPlainS);
            }
            finalKey.printKeyString("Decryption Key:\t");
            finalKey.encipherDecipherAll(ciphertext, plaintext, clen);

            byte[] steppings = new byte[Key.MAXLEN];
            finalKey.showSteppings(steppings, clen);
            String steppingsS = Utils.getCiphertextStringNoXJ(steppings, clen);
            CtAPI.printf("\nPlain Text (Rotor stepping information below plain text): \n\n%s\n%s\n\n", Utils.getString(plaintext, clen), steppingsS);
            CtAPI.printf("Removing Xs and Js: \n\n%s\n\n", Utils.getCiphertextStringNoXJ(plaintext, clen));

            finalKey.initPathLookupAll(clen);
            CtAPI.printf("Plaintext Trigrams Score: %d, Bigrams Score %d, IC: %.5f\n",
                    finalKey.triscore(ciphertext, clen),
                    finalKey.biscore(ciphertext, clen),
                    finalKey.icscore(ciphertext, clen));


        } else {
            Key finalKey = new Key(key);

            System.arraycopy(ciphertext, 0, plaintext, 0, clen); // just for clarity
            if (indicatorS.length() != 0) {
                byte indicCrypt[] = new byte[3];
                byte indicPlain[] = new byte[3];
                String indicCryptS;
                String indicPlainS = indicatorS.toUpperCase();

                key.printKeyString("Key for Indicator:\t");
                CtAPI.printf("\nPlain Indicator:     \t%s\n\n", indicPlainS);
                int ilen = Utils.getText(indicPlainS, indicPlain);
                key.encipherDecipherAll(indicPlain, indicCrypt, ilen);
                indicCryptS = Utils.getString(indicCrypt, ilen);
                CtAPI.printf("Encrypted Indicator: \t%s\n\n", indicCryptS);

                finalKey.setMesg(indicPlainS);

                finalKey.printKeyString("Encrytion Key:\t");
                finalKey.encipherDecipherAll(plaintext, ciphertext, clen);


                CtAPI.printf("\n\nEncrypted Message: \t%d %s %s \n\n%s\n\n",
                        clen, key.getMesg(), indicCryptS, Utils.getCiphertextStringInGroups(ciphertext, clen));
            } else {
                finalKey.printKeyString("Encrytion Key:\t");
                finalKey.encipherDecipherAll(plaintext, ciphertext, clen);


                CtAPI.printf("\nEncrypted Message: \t%d \n\n%s\n\n",
                        clen, Utils.getCiphertextStringInGroups(ciphertext, clen));
            }


        }
    }

    private static boolean bombeSearch(int HILLCLIMBING_CYCLES, String CRIB, int RIGHT_ROTOR_SAMPLING, MRingScope MIDDLE_RING_SCOPE, boolean VERBOSE, String CRIB_POSITION, Mode MODE, boolean range, int THREADS, String indicatorS, String indicatorMessageKeyS, byte[] ciphertext, int clen, Key lowKey, Key highKey, Key key) {
        byte[] crib = new byte[BombeCrib.MAXCRIBL];
        int maxCribLen = Math.min(BombeCrib.MAXCRIBL, clen);
        if (CRIB.length() > maxCribLen) {
            CtAPI.goodbyeError("Crib too long (%d letters) - should not be longer than %d letters\n", CRIB.length(), maxCribLen);
        }
        int crlen = Utils.getText(CRIB, crib);

        int minPos = 0;
        int maxPos = (clen - crlen);
        if (CRIB_POSITION.length() != 0 && !CRIB_POSITION.equalsIgnoreCase("*")) {
            int separator = CRIB_POSITION.indexOf("-");
            if (separator == -1) {
                minPos = getIntValue(CRIB_POSITION, 0, maxPos, Flag.CRIB_POSITION);
                if (minPos == -1)
                    return true;
                else
                    maxPos = minPos;
            } else {
                minPos = getIntValue(CRIB_POSITION.substring(0, separator), 0, maxPos, Flag.CRIB_POSITION);
                if (minPos == -1)
                    return true;
                maxPos = getIntValue(CRIB_POSITION.substring(separator + 1), minPos, maxPos, Flag.CRIB_POSITION);
                if (maxPos == -1)
                    return true;


            }
        }

        int pos = minPos;

        BombeMenu menus[] = new BombeMenu[1000];
        int nMenus = 0;

        while (((pos = BombeCrib.nextValidPosition(ciphertext, clen, crib, crlen, pos)) != -1) && (pos <= maxPos)) {

            BombeCrib bombeCrib = new BombeCrib(ciphertext, crib, crlen, pos, VERBOSE && (minPos == maxPos));

            if ((bombeCrib.menu.score < BombeCrib.BADSCORE) || ((minPos == maxPos))) {
                menus[nMenus++] = bombeCrib.menu;
                CtAPI.printf("Creating Bombe Menu at Position %d (Links: %d, Closures:%d, Score:%.3f)\n",
                        bombeCrib.menu.cribStartPos, bombeCrib.menu.totalItems, bombeCrib.menu.totalClosures, bombeCrib.menu.score);
                if (bombeCrib.menu.score > BombeCrib.BADSCORE)
                    CtAPI.printf("Warning: Turing Score (%.3f) is high (higher means worse) for Bombe menu. This may create many false stops. A longer crib may help.\n",
                            bombeCrib.menu.score);


            }
            pos++;
        }
        if (nMenus > 0) {
            CtAPI.printf("\n %d Bombe menus created - Starting search using Turing Bombe\n\n", nMenus);
            if (!range) {
                lowKey = highKey = key;
            }
            Search.searchCribMenus(menus, nMenus, lowKey, highKey, MIDDLE_RING_SCOPE, RIGHT_ROTOR_SAMPLING,
                        HILLCLIMBING_CYCLES, THREADS, ciphertext, clen, VERBOSE, indicatorS, indicatorMessageKeyS);

        } else
            CtAPI.printf("No good Bombe menu (Turing Score less than %.3f) found for Crib - Either not enough links/closures, or letters encrypted to themselves\n", BombeCrib.BADSCORE);
        return false;
    }

    private static void createCommandLineArguments() {

        final String KEY_FLAG_STRING = "k";
        final String SCENARIO_FLAG_STRING = "z";
        final String CRIB_FLAG_STRING = "p";
        final String CRIB_POSITION_FLAG_STRING = "j";
        final String VERBOSE_FLAG_STRING = "u";
        final String MESSAGE_INDICATOR_FLAG_STRING = "w";
        final String RIGHT_ROTOR_SAMPLING_FLAG_STRING = "x";
        final String MIDDLE_RING_SCOPE_FLAG_STRING = "y";
        final String HILLCLIMBING_CYCLES_FLAG_STRING = "h";

        CommandLine.add(new CommandLineArgument(
                Flag.KEY,
                KEY_FLAG_STRING,
                "Key range or key",
                "Range of keys, or specific key. Examples: range of M3 keys B:532:AAC:AAA-B:532:AAC:ZZZ,\n" +
                        "\t\tsingle M4 key B:B532:AAAC:AJKH, single H key with stecker B:532:AAC:JKH|ACFEHJKOLZ, \n " +
                        "\t\tkey range with stecker B:532:AAC:AAA-B:532:AAC:ZZZ|ACFEHJKOLZ  When a range is specified, \n" +
                        "\t\tthe program will sweep for each field in the key (right to left), from the value specified on the left side of the range \n" +
                        "\t\tuntil it reaches the upper value specified in the right side of the range.\n" +
                        "\t\tFor example, to sweep all ring settings from AAA to AZZ (Model H): \n" +
                        "\t\t-" + KEY_FLAG_STRING + " C:321:AAA:AAA-C:321:AZZ:AAA\n" +
                        "\t\tOr, to test all wheel combinations -" + KEY_FLAG_STRING + " A:111:AAA:AAA-C:555:AZZ:ZZZ\n" +
                        "\t\tOr, to sweep only values for the middle message settings: -" + KEY_FLAG_STRING + " C:321:ABC:HAK-C:321:ABC:HZK\n" +
                        "\t\tOr, to sweep only values for the middle and right wheels (other settings known): -" + KEY_FLAG_STRING + " C:511:ABC:DEF-C:544:ABC:DEF\n" +
                        "\t\tNote that in a range, wheel numbers can be repeated (e.g. -" + KEY_FLAG_STRING + " B:B111:AAAA:AAAA)\n" +
                        "\t\twhile in a single key (-" + KEY_FLAG_STRING + " B:532:AAC:JKH) this is not allowed.\n" +
                        "\t\tKey format for Model H: u:www:rrr:mmm\n" +
                        "\t\t    where u is the reflector (B or C), www are the wheels from left to right (1 to 5, e.g. 321)  \n" +
                        "\t\t    rrr are the ring settings in letters (e.g. AZC) and mmm are the message settings \n" +
                        "\t\tFor Model M3 = u:www:rrr:mmm " +
                        "\t\t    where u is the reflector (B or C), www are the wheels from left to right (1 to 8, e.g. 851)  \n" +
                        "\t\t   rrr are the ring settings in letters (e.g. AZC) and mmm are the message settings \n" +
                        "\t\tfor Model H4 = u:gwww:rrrr:mmmm \n" +
                        "\t\t    where u is the reflector (B), g is the greek wheel (B or G) \n" +
                        "\t\t    www are the wheels from left to right (1 to 8, e.g. 821)  \n" +
                        "\t\t    rrrr are the ring settings in letters (e.g. AAZC) and mmmm are the message settings \n" +
                        "\t\tNote: For models H and M3, it is also possible to specify rings settings with numbers 01 (for a) to 26 (for z).    \n" +
                        "\t\t    for example, -" + KEY_FLAG_STRING + " b:413:021221:abc is equivalent to -" + KEY_FLAG_STRING + " b:413:BLU:abc.   \n" +
                        " ",
                true,
                ""));

        CommandLine.add(new CommandLineArgument(
                Flag.MODEL,
                "m",
                "Enigma model",
                "Enigma Model. H (Army Model), M3 (Navy 3 rotors) or M4 (Navy 4 rotors).",
                false,
                "H",
                new String[]{"H", "M3", "M4",}));

        CommandLine.add(new CommandLineArgument(
                Flag.MODE,
                "o",
                "Search mode",
                "Search mode (for the case these is no crib). \n" +
                        "\t\t\tHC for hillclimbing to search for best steckers at each possible rotor setting\n" +
                        "\t\t\tIC or TRIGRAMS for Index of Coincidence/trigram measurements at each possible rotor setting. \n" +
                        "\t\t\t(For IC or TRIGRAMS, the steckers must be specificed in -" + KEY_FLAG_STRING + ").\n" +
                        "\t\t\tBOMBE for an implementation of the Turing Bombe, crib attack. \n" +
                        "\t\t\tINDICATORS for an attack on 1930-1938 double indicators (extension of Rejewski's method).\n" +
                        "\t\t\tINDICATORS1938 for an attack on 1938-1940 double indicators (extension of Zygalski's method).\n" +
                        "\t\t\tSCENARIO to create siumlated scenarios (see -" + SCENARIO_FLAG_STRING + ").\n" +
                        "\t\t\tDECRYPT (or ommit) for simple decryption mode.\n",
                false,
                "DECRYPT",
                new String[]{"HILLCLIMBING", "IC", "TRIGRAMS", "BOMBE", "INDICATORS", "INDICATORS1938", "SCENARIO", "DECRYPT",}));

        CommandLine.add(new CommandLineArgument(
                Flag.CIPHERTEXT,
                "i",
                "Ciphertext or ciphertext file",
                "Ciphertext string, or full path for the file with the cipher, ending with .txt.",
                false,
                ""));

        CommandLine.add(new CommandLineArgument(
                Flag.CRIB,
                CRIB_FLAG_STRING,
                "Crib (known plaintext)",
                "Known plaintext (crib) for attack using extended Turing Bombe.\n" +
                        "\t\tThe position of the crib may be specified with -" + CRIB_POSITION_FLAG_STRING + ". \n" +
                        "\t\tTo exclude one or more of the letters from menus, replace the crib letter with a ? \n" +
                        "\t\tFor example -" + CRIB_FLAG_STRING + " eins???zwo still specifies a crib of 10 letters but no menu links will be created for the 3 letters marked as ?.\n" +
                        "\t\tThe details of the menus can be printed using-" + VERBOSE_FLAG_STRING + " (but only if a single key is given with -" + KEY_FLAG_STRING + ", and not a range).",
                false,
                ""));

        CommandLine.add(new CommandLineArgument(
                Flag.CRIB_POSITION,
                CRIB_POSITION_FLAG_STRING,
                "Crib start position",
                "Starting position of crib, or range of starting positions. 0 means first letter. Examples: " +
                        "\t\t\t-" + CRIB_POSITION_FLAG_STRING + " 0 if crib starts at first letter,\n" +
                        "\t\t\t-" + CRIB_POSITION_FLAG_STRING + " 10 if crib starts at the 11th letter, \n+" +
                        "\t\t\t-" + CRIB_POSITION_FLAG_STRING + " 0-9 if crib may start at any of the first 10 positions,\n" +
                        "\t\t\t-" + CRIB_POSITION_FLAG_STRING + " * if crib may start at any position.\n" +
                        "\t\tPosition(s) generating a menu conflict (letter encrypted to itself) are discarded. \n",
                false,
                "0"));

        CommandLine.add(new CommandLineArgument(
                Flag.INDICATORS_FILE,
                "d",
                "File with indicators",
                "File with set of indicators. The file should contain either groups of 6 letters, or group of 9 letters. \n" +
                        "\t\tIf groups of encrypted double indicators with 6 letters are given, searches key according to the Cycle Characteristic method developed by the Poles before WWII.\n" +
                        "\t\tAssumption: the basic daily key included a basic message key, used to encrypt doubled indicators. A database of such  encrypted double indicators is required.\n" +
                        "\t\tFinds the Ring/Wheel order and the message key which creates cycles which match those of the database. Then Hill Climbs to find stecker plugs which match the all indicators \n" +
                        "\t\t(after decryption, will show them doubled). The last step is to take the first (decrypted) indicator as the Message Key for the message  \n" +
                        "\t\titself and perform a Trigram based search to find the Ring Settings and to decipher it. \n" +
                        "                \n" +
                        "\t\tIf groups of encrypted double indicators with of 3+6=9 letters are given, \n" +
                        "\t\tfinds full key according to the Zygalski's Sheets method developed by the Poles before WWII.\n" +
                        "\t\tIndicators include 3 letters for the key to encrypt the double message key, and 6 letters of the doubled encrypted message.\n" +
                        "\t\tWill search for keys (Wheel order, Ring settings) which together with the keys in the indicator groups,\n" +
                        "\t\tcreate 'female' patterns which match the database (those keys with females), as it was done with the original Zygalski's Sheets\n" +
                        "\t\tAfter ring settings are found, Stecker Board settings are also detected, and the first key indicator (from the file)\n" +
                        "\t\tis used to decipher the message.\n",
                false,
                "indicators.txt"));

        CommandLine.add(new CommandLineArgument(
                Flag.MESSAGE_INDICATOR,
                MESSAGE_INDICATOR_FLAG_STRING,
                "Message indicator options",
                "Indicator sent with the ciphertext. Has two distinct purposes and forms: \n" +
                        "\t\t-w {Encrypted Indicator} e.g.-" + MESSAGE_INDICATOR_FLAG_STRING + " STG.  This must be used together with a single key in -" + KEY_FLAG_STRING + " in which the steckers were specified (e.g. -" + KEY_FLAG_STRING + " B:532:AAC:JKH:ACFEHJKOLZ). \n" +
                        "\t\t    First, this indicator is decrypted using the given key (daily key), then the decrypted indicator is used as the message key to decrypt the full message. \n" +
                        "\t\t-w {Indicator Message Key}:{Encrypted Indicator} e.g.-" + MESSAGE_INDICATOR_FLAG_STRING + " OWL:STG. In this form, this is used as an additional filter when searching for the best key (except for hillclimbing).\n" +
                        "\t\t   Only messages keys which are a result of decrypting the Encrypted Indicator with the given Indicator Message key are considered for the search\n" +
                        "\t\t   Stecker board settings must be known and specified (e.g. B:532:AAA:AAA-B:532:AAZ:ZZZ|ACFEHJKOLZ). Not compatible with HILLCLIMBING mode\n",

                false,
                ""));

        CommandLine.add(new CommandLineArgument(
                Flag.RESOURCE_PATH,
                "r",
                "Resource directory",
                "Full path of directory for resources (e.g. stats files).",
                false,
                "."));

        CommandLine.add(new CommandLineArgument(
                Flag.THREADS,
                "t",
                "Number of processing threads",
                "Number of threads, for multithreading. 1 for no multithreading.",
                false,
                1, 20, 7));

        CommandLine.add(new CommandLineArgument(
                Flag.HILLCLIMBING_CYCLES,
                HILLCLIMBING_CYCLES_FLAG_STRING,
                "Number of hillclimbing cycles",
                "Number of hillclimbing cycles. 0 for no hillclimbing.",
                false,
                0, 1000, 2));

        CommandLine.add(new CommandLineArgument(
                Flag.LANGUAGE,
                "g",
                "Language",
                "Language used for statistics and for simulation random text.",
                false,
                "GERMAN",
                new String[]{"GERMAN", "ENGLISH",}));

        CommandLine.add(new CommandLineArgument(
                Flag.SIMULATION,
                "s",
                "Simulation",
                "Create ciphertext from random key and plaintext from book file."));

        CommandLine.add(new CommandLineArgument(
                Flag.SIMULATION_TEXT_LENGTH,
                "l",
                "Length of text for simulation",
                "Length of random plaintext encrypted for simulation.",
                false,
                10, 1000, 100));

        CommandLine.add(new CommandLineArgument(
                Flag.VERBOSE,
                VERBOSE_FLAG_STRING,
                "Verbose",
                "Show details of attack"));

        CommandLine.add(new CommandLineArgument(
                Flag.RIGHT_ROTOR_SAMPLING,
                RIGHT_ROTOR_SAMPLING_FLAG_STRING,
                "Left rotor sampling interval.",
                "Check only a sample of left rotor positions.-" + RIGHT_ROTOR_SAMPLING_FLAG_STRING + " {right rotor interval value} {default - 1 - no sampling, check all positions in range}.\n" +
                        "\t\tIf the interval > 1, allows for skipping some right rotor positions in searches.\n" +
                        "\t\tFor example -X 3 means that only one in three Right Rotor positions will be tested.  \n" +
                        "\t\tDue to redundant states in the Enigma encryption process, this is likely to still produce a key\n" +
                        "\t\twhich will decrypt most of the text. \n" +
                        "\t\tShould be used with caution together with mode BOMBE (Bombe search for menu stops) as this may cause stops to be missed. \n",
                false,
                1, 5, 1));
        CommandLine.add(new CommandLineArgument(
                Flag.MIDDLE_RING_SCOPE,
                MIDDLE_RING_SCOPE_FLAG_STRING,
                "Optimize middle rotor moves",
                "Optimize middle rotor moves.-" + MIDDLE_RING_SCOPE_FLAG_STRING + " {option} {default - 0 - no stepping optimization}.\n" +
                        "\t\tReduce the number of middle rotor settings (which generates stepping of left rotor) to be tested. \n" +
                        "\t\t-" + MIDDLE_RING_SCOPE_FLAG_STRING + " 0 - No reduction, all Middle Rotors settings specified in the range will be tested.\n" +
                        "\t\t-" + MIDDLE_RING_SCOPE_FLAG_STRING + " 1 - Test all Middle Rotor settings which generate a stepping of the Left Rotor, plus one settings which doesn't. \n" +
                        "\t\t       Reliable, no valid solutions will be missed, and reduces scope from 26 to {message length}/26+1 \n" +
                        "\t\t-" + MIDDLE_RING_SCOPE_FLAG_STRING + " 2 - Test all Middle Rotor settings which generate a stepping of the Left Rotor affecting the first 1/5 or last 1/5 of the message, plus one more \n" +
                        "\t\t       settings which is not generating a stepping. This is good compromise between speed and accuracy. Reduces scope from 26 to {message length}*0.40/26+1 \n" +
                        "\t\t-" + MIDDLE_RING_SCOPE_FLAG_STRING + " 3 - Test a single Middle Rotor settings which DOES NOT generate a stepping of the Left Rotor. Fastest and most agressive option since only one Middle Rotor setting\n" +
                        "\t\t       will be tested, but part of the message may be garbled is there was such a stepping originally. Good for short messages since probablity \n" +
                        "\t\t       for Left Rotor stepping is {message length}/676. -Z 3 can save a lot of time if successful. \n" +
                        "\t\t-" + MIDDLE_RING_SCOPE_FLAG_STRING + " 4 - Test only all Middle Rotor settings which generate a stepping of the Left Rotor. \n" +
                        "\t\t       Usually not needed except for testing purposes. Reduces scope from 26 to {message length}/26 \n" +
                        "\t\t-" + MIDDLE_RING_SCOPE_FLAG_STRING + " 5 - Test all Middle Rotor settings which do NOT generate a stepping of the Left Rotor. \n" +
                        "\t\tNote: If a range is specified with a low position different from A, and high position different from Z, only-" + MIDDLE_RING_SCOPE_FLAG_STRING + " 0 is allowed. \n",
                false,
                0, 5, 0));

        CommandLine.add(new CommandLineArgument(
                Flag.SCENARIO,
                SCENARIO_FLAG_STRING,
                "Generate random scenario",
                "Generates simulated indicators and cryptogram(s).\n" +
                        "\t\tA range of keys must be selected (-" + KEY_FLAG_STRING + ") and a random key within the range will be selected. \n" +
                        "\t\tThe format for the options is: {f}:{l}:{n}:{s}:{g}:{c}. \n" +
                        "\t\t{f} = format: 1 for regular post 1940 format , 2 for 1938-40 format with doubled indicator (Solvable with Zygalski's Sheets method), 3 for pre-1938 with same key for all day \n" +
                        "\t\t    solvable with the Cycle Pattern match method.  Default is 1. Formats (2) and (3) are not compatible with split messages\n" +
                        "\t\t{l} is the total length of the random messages (default 150). For format 0 (non-random this is the length of each message. \n" +
                        "\t\t{n} is the number of messages (text will be split) for Format 1, for format 2&3 this is the number of indicators\n" +
                        "\t\t{s} is the number of Stecker Plugs (default 10) {g} is the percentage of garbled letters (default 0) {c} is the length of a random crib   \n" +
                        "\t\t\n" +
                        "\t\tThe following files are created: \n" +
                        "\t\t(1) 'cipher.txt' ciphertext for a single message (not split)  \n" +
                        "\t\t    In case of message split messages several files (cipher1.txt, cipher2.txt etc.. will be created). \n" +
                        "\t\t(2) if format is 2 (1938-1940 or 2 (pre 1938) a file named 'indicators.txt' is created. \n" +
                        "\t\t    The indicator used for the message in (1) is the first. In format 3, groups of 6 letters (encrypted doubled keys)  \n" +
                        "\t\t    are kept. In format 3, groups of 9 letters (indicator plus encrypted doubled key) are kept \n" +
                        "\t\t(3) a file named 'challenge.txt' which containts all elements of the challenge (messages with headers, crib, etc)   \n" +
                        "\t\t(4) a file named 'solution.txt' which contains all elements of the solution.  \n" +
                        "\t\tExample -" + SCENARIO_FLAG_STRING + " 1:500:3:10:3: means post 1940 format, total length 500 split into 3 messages, 10 plugs, 3 precent garbled letters, no crib (default)\n" +
                        "\t\tExample -" + SCENARIO_FLAG_STRING + " :::::25 means post 1940 format (default), length 150 (default), no split(default), 10 plugs (the default), crib of length 25\n",

                false,
                ""));
        CommandLine.add(new CommandLineArgument(
                Flag.CYCLES,
                "n",
                "Reserved",
                "Reserved.",
                false,
                0, 1000, 0));

    }

    private static int getIntValue(String s, int min, int max, Flag flag) {

        for (int i = 0; i < s.length(); i++) {
            if (Utils.getDigitIndex(s.charAt(i)) == -1) {
                CtAPI.goodbyeError("Invalid %s (%s) for %s - Expecting number from %d to %d \n", s, CommandLine.getShortDesc(flag), CommandLine.getFlagString(flag), min, max);
            }
        }

        int intValue = Integer.parseInt(s);

        if ((intValue >= min) && (intValue <= max)) {
            return intValue;
        }
        CtAPI.goodbyeError("Invalid %s (%s) for %s - Expecting number from %d to %d \n", s, CommandLine.getShortDesc(flag), CommandLine.getFlagString(flag), min, max);
        return -1;

    }

}
