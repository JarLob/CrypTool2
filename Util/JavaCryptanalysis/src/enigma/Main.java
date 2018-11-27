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

    public static void main(String[] args) {

        createCommandLineArguments();
        //CommandLineArgument.printUsage();
        BestResults.setDiscardSamePlaintexts(false);
        BestResults.setThrottle(false);
        BestResults.setMaxNumberOfResults(10);

        CtAPI.open("Enigma attacks", "1.0");

        CommandLine.parseAndPrintCommandLineArgs(args);

        final String RESOURCE_PATH = CommandLine.getStringValue(Flag.RESOURCE_PATH);
        final String SCENARIO_PATH = CommandLine.getStringValue(Flag.SCENARIO_PATH);
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
            BombeSearch.bombeSearch(CRIB, ciphertext, clen, range, lowKey, highKey, key, indicatorS, indicatorMessageKeyS, HILLCLIMBING_CYCLES, RIGHT_ROTOR_SAMPLING, MIDDLE_RING_SCOPE, VERBOSE, CRIB_POSITION, THREADS);
        } else if (MODE == Mode.DECRYPT && !range) {
            encryptDecrypt(indicatorS, plaintext, ciphertext, clen, key);
        } else if (MODE == Mode.IC) {
            TrigramICSearch.searchTrigramIC(lowKey, highKey, true, MIDDLE_RING_SCOPE, RIGHT_ROTOR_SAMPLING, false, HILLCLIMBING_CYCLES, 0, THREADS, ciphertext, clen, indicatorS, indicatorMessageKeyS);
        } else if (MODE == Mode.TRIGRAMS) {
            TrigramICSearch.searchTrigramIC(lowKey, highKey, false, MIDDLE_RING_SCOPE, RIGHT_ROTOR_SAMPLING, false, HILLCLIMBING_CYCLES, 0, THREADS, ciphertext, clen, indicatorS, indicatorMessageKeyS);
        } else if (MODE == Mode.HILLCLIMBING) {
            HillClimb.hillClimbRange(range ? lowKey : key, range ? highKey : key, HILLCLIMBING_CYCLES, THREADS, 0, MIDDLE_RING_SCOPE, RIGHT_ROTOR_SAMPLING, ciphertext, clen);
        } else if (MODE == Mode.SCENARIO) {
            new RandomChallenges(SCENARIO_PATH, RESOURCE_PATH + "\\faust.txt", lowKey, highKey, SCENARIO);
        } else if (MODE == Mode.INDICATORS) { // cycles
            IndicatorsSearch.indicatorsSearch(INDICATORS_FILE, lowKey, highKey, steckerS, ciphertext, clen, RIGHT_ROTOR_SAMPLING, MIDDLE_RING_SCOPE, HILLCLIMBING_CYCLES, THREADS);
        } else if (MODE == Mode.INDICATORS1938) {
            Indicators1938Search.indicators1938Search(INDICATORS_FILE, lowKey, highKey, steckerS, ciphertext, clen);
        }

        CtAPI.goodbye();

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

    private static void createCommandLineArguments() {

        final String KEY_FLAG_STRING = "k";
        final String SCENARIO_FLAG_STRING = "z";
        final String SCENARIO_PATH_FLAG_STRING = "f";
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
                        "\t\tkey range with stecker B:532:AAC:AAA-B:532:AAC:ZZZ|ACFEHJKOLZ. When a range is specified, \n" +
                        "\t\tthe program will sweep for each field in the key (right to left), from the value specified on the left side of the range \n" +
                        "\t\tuntil it reaches the upper value specified in the right side of the range.\n" +
                        "\t\tFor example, to sweep all ring settings from AAA to AZZ (Model H): \n" +
                        "\t\t-" + KEY_FLAG_STRING + " C:321:AAA:AAA-C:321:AZZ:AAA\n" +
                        "\t\tOr, to test all wheel combinations -" + KEY_FLAG_STRING + " A:111:AAA:AAA-C:555:AZZ:ZZZ\n" +
                        "\t\tOr, to sweep only values for the middle message settings: -" + KEY_FLAG_STRING + " C:321:ABC:HAK-C:321:ABC:HZK\n" +
                        "\t\tOr, to sweep only values for the middle and right wheels (other settings known): -" + KEY_FLAG_STRING + " C:521:ABC:DEF-C:521:ABC:DEF\n" +
                        "\t\tNote that in a range, wheel numbers can be repeated (e.g. -" + KEY_FLAG_STRING + " B:B111:AAAA:AAAA-B:B555:AAAA:ZZZZ)\n" +
                        "\t\twhile in a single key this is not allowed (-" + KEY_FLAG_STRING + " B:522:AAC:JKH is invalid).\n" +
                        "\t\tKey format for Model H: u:www:rrr:mmm\n" +
                        "\t\t    where u is the reflector (A, B, or C), www are the 3 wheels from left to right (1 to 5, e.g. 321)  \n" +
                        "\t\t    rrr are the ring settings (e.g. AZC) and mmm are the message settings \n" +
                        "\t\tFor Model M3 = u:www:rrr:mmm \n" +
                        "\t\t    where u is the reflector (B or C), www are the 3 wheels from left to right (1 to 8, e.g. 851)  \n" +
                        "\t\t    rrr are the ring settings (e.g. AZC) and mmm are the message settings \n" +
                        "\t\tfor Model H4 = u:gwww:rrrr:mmmm \n" +
                        "\t\t    where u is the reflector (B), g is the greek wheel (B or G) \n" +
                        "\t\t    www are the wheels from left to right (1 to 8, e.g. 821)  \n" +
                        "\t\t    rrrr are the ring settings (e.g. AAZC) and mmmm are the message settings \n" +
                        "\t\tNote: For models H and M3, it is also possible to specify rings settings with numbers 01 to 26 (instead of A to Z).    \n" +
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
                        "\t\t\tHC for hillclimbing to search for best steckers at each possible rotor setting (Weierud/Krah's method).\n" +
                        "\t\t\tTRIGRAMS look for rotor settings with best trigram score. The steckers must be specificed in -" + KEY_FLAG_STRING + ",\n" +
                        "\t\t\t   e.g. -" + KEY_FLAG_STRING + "B:132:AAC:AAA-B:132:AAC:ZZZ|ACFEHJKOLZ.\n" +
                        "\t\t\tIC look for rotor settings with best Index of Coincidence. For cryptograms less than 500 letters, \n" +
                        "\t\t\t   the steckers must be specificed in -" + KEY_FLAG_STRING + ", e.g. -" + KEY_FLAG_STRING + "B:132:AAC:AAA-B:132:AAC:ZZZ|ACFEHJKOLZ.\n" +
                        "\t\t\tBOMBE for crib/known-plaintext attach (extension of the Turing Bombe). \n" +
                        "\t\t\tINDICATORS for an attack on 1930-1938 double indicators (extension of Rejewski's method).\n" +
                        "\t\t\tINDICATORS1938 for an attack on 1938-1940 double indicators (extension of Zygalski's method).\n" +
                        "\t\t\tSCENARIO to create a simulated ciphertext/plaintext/indicators scenario (see -" + SCENARIO_FLAG_STRING + "and -" +  SCENARIO_PATH_FLAG_STRING +" options).\n" +
                        "\t\t\tDECRYPT for simple decryption.\n",
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
                        "\t\tTo exclude one or more of the letters from menus, replace each unknown crib letter with a ? symbol\n" +
                        "\t\tFor example -" + CRIB_FLAG_STRING + " eins???zwo specifies a crib of 10 letters but no menu links will be created for the 3 letters marked as ?.\n" +
                        "\t\tThe details of the menus can be printed using-" + VERBOSE_FLAG_STRING + " (only if a single key is given with -" + KEY_FLAG_STRING + ", and not a range).",
                false,
                ""));

        CommandLine.add(new CommandLineArgument(
                Flag.CRIB_POSITION,
                CRIB_POSITION_FLAG_STRING,
                "Crib start position",
                "Starting position of crib, or range of possible starting positions. 0 means first letter. Examples: \n" +
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
                "Full file path for indicators file.",
                "File with set of indicators. The file should contain either groups of 6 letters (INDICATORS mode), or groups of 9 letters (INIDCATORS1938 mode). \n" +
                        "\t\tIf groups of encrypted double indicators with 6 letters are given, searches key according to the Cycle Characteristic method developed by the Rejewski before WWII.\n" +
                        "\t\t  Finds the daily key which creates cycles which match those of the database. Then finds stecker plugs which match the all indicators \n" +
                        "\t\t  If a ciphertext is provided, using the first (decrypted) indicator as the Message Key for that message  \n" +
                        "\t\t  perform a trigram-based search to find the Ring Settings and to decipher the message. \n" +
                        "\t\tIf groups of encrypted double indicators with of 3+6=9 letters are given, \n" +
                        "\t\t  finds daily key according to the Zygalski's Sheets method developed by the Poles before WWII.\n" +
                        "\t\t  Indicators include 3 letters for the key to encrypt the double message key, and 6 letters of the doubled encrypted message.\n" +
                        "\t\t  Will search for keys (wheel order, wing settings) which together with the keys in the indicator groups,\n" +
                        "\t\t  create 'female' patterns which match the database (those keys with females). Stecker settings are also detected, and the first key indicator (from the file)\n" +
                        "\t\t is used to decipher the ciphertext (if a ciphertext was provided).\n",
                false,
                ""));

        CommandLine.add(new CommandLineArgument(
                Flag.MESSAGE_INDICATOR,
                MESSAGE_INDICATOR_FLAG_STRING,
                "Message indicator options",
                "Indicator sent with the ciphertext. Has two distinct purposes and forms: \n" +
                        "\t\t-w {3-letter encrypted indicator} e.g.-" + MESSAGE_INDICATOR_FLAG_STRING + " STG.  This must be used together with a single key in -" + KEY_FLAG_STRING + " in which the steckers were specified (e.g. -" + KEY_FLAG_STRING + " B:532:AAC:JKH:ACFEHJKOLZ). \n" +
                        "\t\t    First, this indicator is decrypted using the given key (daily key), then the decrypted indicator is used as the message key to decrypt the full message. \n" +
                        "\t\t-w {3-letter message key}:{3-letter encrypted indicator} e.g.-" + MESSAGE_INDICATOR_FLAG_STRING + " OWL:STG. In this form, this is used as an additional filter when searching for the best key (except for hillclimbing).\n" +
                        "\t\t   Only messages keys which are a result of decrypting the encrypted indicator with the given message key are considered for the search.\n" +
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
                Flag.VERBOSE,
                VERBOSE_FLAG_STRING,
                "Verbose",
                "Show details of crib attack."));

        CommandLine.add(new CommandLineArgument(
                Flag.RIGHT_ROTOR_SAMPLING,
                RIGHT_ROTOR_SAMPLING_FLAG_STRING,
                "Left rotor sampling interval.",
                "Check only a sample of left rotor positions.-" + RIGHT_ROTOR_SAMPLING_FLAG_STRING + " {right rotor interval value} {default - 1 - no sampling, check all positions in range}.\n" +
                        "\t\tIf the interval > 1, test only a sample of right rotor positions in search.\n" +
                        "\t\tFor example -" + RIGHT_ROTOR_SAMPLING_FLAG_STRING + " 3 means that only one in three right rotor positions will be tested.  \n" +
                        "\t\tDue to redundant states in the Enigma encryption process, this is likely to still produce a partial or full decryption. \n" +
                        "\t\tShould be used with caution together with mode BOMBE (Bombe search for menu stops) as this may cause stops to be missed. \n",
                false,
                1, 5, 1));
        CommandLine.add(new CommandLineArgument(
                Flag.MIDDLE_RING_SCOPE,
                MIDDLE_RING_SCOPE_FLAG_STRING,
                "Optimize middle rotor moves",
                "Optimize middle rotor moves.-" + MIDDLE_RING_SCOPE_FLAG_STRING + " {option} {default - 0 - no optimization}.\n" +
                        "\t\tReduce the number of middle rotor settings to be tested. \n" +
                        "\t\t-" + MIDDLE_RING_SCOPE_FLAG_STRING + " 0 - No reduction, all middle rotor settings specified in the range will be tested.\n" +
                        "\t\t-" + MIDDLE_RING_SCOPE_FLAG_STRING + " 1 - Test all middle rotor settings which generate a stepping of the left rotor, plus one settings which does NOT. \n" +
                        "\t\t       Reliable, no valid solutions will be missed, and reduces scope from 26 to {message length}/26+1 \n" +
                        "\t\t-" + MIDDLE_RING_SCOPE_FLAG_STRING + " 2 - Test all middle rotor settings which generate a stepping of the left rotor affecting the first 1/5 or last 1/5 of the message, plus one more \n" +
                        "\t\t       setting which is not generating a stepping. This is a good compromise between speed and accuracy. Reduces scope from 26 to {message length}*0.40/26+1 \n" +
                        "\t\t-" + MIDDLE_RING_SCOPE_FLAG_STRING + " 3 - Test one middle rotor setting which does NOT generate a stepping of the left rotor. Fastest and most agressive option since only one middle rotor setting\n" +
                        "\t\t       will be tested, but part of the message may be garbled if there was such a stepping originally. Good for short messages since probablity \n" +
                        "\t\t       for left rotor stepping is {message length}/676. Can save a lot of search time if successful. \n" +
                        "\t\t-" + MIDDLE_RING_SCOPE_FLAG_STRING + " 4 - Test only all middle rotor settings which generate a stepping of the left rotor. \n" +
                        "\t\t       Usually not needed except for testing purposes. Reduces scope from 26 to {message length}/26 \n" +
                        "\t\t-" + MIDDLE_RING_SCOPE_FLAG_STRING + " 5 - Test all middle rotor settings which do NOT generate a stepping of the left rotor. \n" +
                        "\t\tNote: The key range should specify the full range (A to Z) for the middle rotor, for any option other then 0. \n",
                false,
                0, 5, 0));

        CommandLine.add(new CommandLineArgument(
                Flag.SCENARIO,
                SCENARIO_FLAG_STRING,
                "Generate random scenario",
                "Generate simulated ciphertext and indicators.\n" +
                        "\t\tA range of keys must be selected (-" + KEY_FLAG_STRING + ") from which a key is randomly selected for simulation. \n" +
                        "\t\tUsage: -"+ SCENARIO_FLAG_STRING + " {f}:{l}:{n}:{s}:{g}:{c}. \n" +
                        "\t\t{f} is the selected scenario: 1 to only generate a ciphertext, 2 for pre-1938 indicators (and a ciphertext), 3 to generate post 1938 doubled indicators (and a ciphertext). \n" +
                        "\t\t    Default is 1. Scenario 2 and 3 are not compatible with the {n} option.\n" +
                        "\t\t{l} is the length of the random ciphertext, or the combined length of all ciphertexts (default 150).\n" +
                        "\t\t{n} is the number of messages (a longer text will be split) for scenario 1, for scenario 2  and 3 this is the number of indicators to be generated.\n" +
                        "\t\t{s} is the number of Stecker Plugs (default 10).\n"+
                        "\t\t{g} is the percentage of garbled letters (default 0).\n"+
                        "\t\t{c} is the length of a crib (the plaintext at the beginning of the message).\n" +
                        "\t\t\n" +
                        "\t\tThe following files are created (<id> is the randomly generated scenario id): \n" +
                        "\t\t -'S<id>cipher.txt' ciphertext for a single message (not split)  \n" +
                        "\t\t   In case of long message which has been split several files (S<id>cipher1.txt, S<id>cipher2.txt etc.. will also be created). \n" +
                        "\t\t - S<id>indicators.txt with indicators, for scenario 2 (1938-1940) or 3 (pre-1938). \n" +
                        "\t\t   With scenario 2, the file contains groups of 6 letters (encrypted doubled keys).  \n" +
                        "\t\t   With scenario 3, it contains groups of 9 letters (indicator plus encrypted doubled key) are kept \n" +
                        "\t\t   The indicator used for the generated ciphertext is the first in that set. \n" +
                        "\t\t - S<id>plaintext.txt contains the plaintext. \n" +
                        "\t\t - S<id>challenge.txt contains all the elements of the challenge (messages with headers, crib, etc) without the solution.\n" +
                        "\t\t - S<id>solution.txt contains all the elements of the solution. \n" +
                        "\t\tExample: -" + SCENARIO_FLAG_STRING + " 1:500:3:10:3:25 - generate plaintexts/ciphertexts, with total length of 500 split into 3 messages.\n" +
                        "\t\t   The stecker board has 10 plugs, 3 percent of the letters are garbled, a crib of 25 letters is given\n" +
                        "\t\tExample: -" + SCENARIO_FLAG_STRING + " 2:50::6::0 - generate 50 pre-1938 doubled indicators, a single plaintext/ciphertext with 150 letters (default). \n" +
                        "\t\t   The stecker board has 6 plugs, no letters are garbled (default), no crib is given.\n",

                false,
                ""));
        CommandLine.add(new CommandLineArgument(
                Flag.SCENARIO_PATH,
                SCENARIO_PATH_FLAG_STRING,
                "Directory for scenario output files",
                "Full path of directory for files created in SCENARIO mode (using the -" + SCENARIO_FLAG_STRING + " option to specify the parameters).",
                false,
                "."));

        CommandLine.add(new CommandLineArgument(
                Flag.CYCLES,
                "n",
                "Reserved",
                "",
                false,
                0, 1000, 0));

    }


    private static void incompatible(Mode mode, Flag[] flags) {
        for (Flag flag : flags) {
            if (CommandLine.isSet(flag)) {
                CtAPI.goodbyeError("Option -%s (%s) not supported for mode %s\n", CommandLine.getFlagString(flag), CommandLine.getShortDesc(flag), mode);
            }
        }
    }

    private static void required(Mode mode, Key.Model currentModel, Key.Model[] models) {
        for (Key.Model model : models) {
            if (model == currentModel) {
                return;
            }
        }
        CtAPI.goodbyeError("Mode %s not supported for model %s\n", mode, currentModel);
    }

    private static void required(Mode mode, Flag[] flags) {
        for (Flag flag : flags) {
            if (!CommandLine.isSet(flag)) {
                CtAPI.goodbyeError("Option -%s (%s) is mandatory with mode %s\n", CommandLine.getFlagString(flag), CommandLine.getShortDesc(flag), mode);
            }
        }
    }

    private static void incompatibleWithRangeOkKeys(Flag[] flags) {
        for (Flag flag : flags) {
            if (CommandLine.isSet(flag)) {
                CtAPI.goodbyeError("Option -%s (%s) not supported for key range\n", CommandLine.getFlagString(flag), CommandLine.getShortDesc(flag));
            }
        }
    }

    private static void incompatibleWithSingleKey(Flag[] flags) {
        for (Flag flag : flags) {
            if (CommandLine.isSet(flag)) {
                CtAPI.goodbyeError("Option -%s (%s) requires a key range\n", CommandLine.getFlagString(flag), CommandLine.getShortDesc(flag));
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



}