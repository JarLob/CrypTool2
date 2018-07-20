package multiplex;

import common.*;

import java.util.ArrayList;

class Main {

    /**
     * Simulation with M94.
     * @param len - length of ciphertext/plaintext. If sweep = true, must be 75.
     * @param sweep - test all 3 first offsets, from 0 to 24 each
     * @param saCycles - number of Simulated Annealing cycles (only when sweep is true).
     */
    private static void simulatedM94(String bookFile, int len, boolean sweep, int saCycles, int threads) {
        if (sweep) {
            if (len != 75) {
                CtAPI.goodbye(-1, "Length must be 75 when 'sweep' is true");
            }
        }
        int[] p = new int[len];
        int[] c = new int[len];
        Utils.readPlaintextSegmentFromFile(bookFile, Utils.randomGet(50000), p);
        M94 encryptionKey = new M94(p.length / 25);
        encryptionKey.randomizeKey();
        encryptionKey.randomizeOffsets();

        encryptionKey.encrypt(p, c);
        encryptionKey.setCipher(c);
        long realMultiplexScore = encryptionKey.eval();
        BestResults.setOriginal(encryptionKey.eval(), encryptionKey.toString(), Utils.getString(encryptionKey.decryption), "Original");
        CtAPI.printf("%s\n%s\n%s\nLength: %d\nTarget Score: %d\n", Utils.getString(p),Utils.getString(c), encryptionKey, len, encryptionKey.eval());
        long start = System.currentTimeMillis();

        if (!sweep) {
            M94 m94 = new M94(encryptionKey.offsets);
            m94.setCipher(c);
            SolveM94.sa(m94, encryptionKey, realMultiplexScore, 0, true);
            CtAPI.printf("%d\n", System.currentTimeMillis() - start);
        } else {
            SolveM94.saSweepOffsets(c, null, saCycles, 1, threads, encryptionKey);
        }
    }

    private static void challengeM94KnownOffsets(String cipherStr, String cribStr, int saCycles, ArrayList<Integer> offsets) {

        int[] c = Utils.getText(cipherStr);

        M94 m94 = new M94(offsets.size());
        m94.setCipherAndCrib(c, cribStr);

        int requiredOffsets = (cipherStr.length() + 24)/25;
        if (offsets.size() < requiredOffsets) {
            CtAPI.goodbye(-1, "Not enough known offsets - need " + requiredOffsets + " only got " + offsets.size());
        }
        for (int i = 0; i < offsets.size(); i++) {
            m94.setOffset(i, offsets.get(i));
        }

        SolveM94.sa(m94, null, -1, saCycles, true);

    }

    /**
     * M94 challenge
     * @param cipherStr - ciphertext string -  must have 75 symbols.
     * @param saCycles - number of Simulated Annealing cycles.
     */
    private static void challengeM94(String cipherStr, String cribStr, int saCycles, int internalSaCycles, int threads) {

        if (cipherStr.length() != 75) {
            CtAPI.goodbye(-1, "M94 attack - length must be exactly 75 if offsets are unknown");
        }
        int len = cipherStr.length();
        int[] c = Utils.getText(cipherStr);
        SolveM94.saSweepOffsets(c, cribStr, saCycles, internalSaCycles, threads, null);
    }

    /**
     * Simulated M138
     * @param len - length of ciphertext.
     * @param sweepOffset - if true, test all possible offsets. If false, test only the correct offset.
     * @param withCrib - if true, find the key matching the real plaintext.
     */
    static void simulatedM138(String bookFile, int len, boolean sweepOffset, boolean withCrib) {
        int[] p = new int[len];
        Utils.readPlaintextSegmentFromFile(bookFile, Utils.randomGet(50000), p);
        int[] c = new int[len];
        M138 encryptionKey = new M138();
        encryptionKey.randomizeKey();
        encryptionKey.randomizeOffset();
        encryptionKey.encrypt(p, c);

       SolveM138.solveM138Challenge(sweepOffset ? 0 : encryptionKey.offset, Utils.getString(c), withCrib ? Utils.getString(p) : "", 0, encryptionKey.key, encryptionKey.offset);
    }

    /**
     * Klaus Schmeh challenge 3
     * @param len - 125 (original length) or less.
     * @param sweepOffset - if true, test all possible offsets. If false, test only the correct offset.
     * @param withCrib - if true, find the key matching the real plaintext.
     * */
    private static void challenge3M138(int len, boolean sweepOffset, boolean withCrib) {
        String challengeString = "RIGVRXIXRHZQOGDQYIXVHCZKJLCDKUSGNDPPIBCLGPZBRUTRFJXHTNQPHWXGQAXPKEEEKMDPWFKSDTLKPTFIXIRUXNTIMTZQQCQOSOPFBXFMMZPSIGZSANJKYHWIO".substring(0, len);
        String crib = "INTHEEARLYNINETEENTWENTIESTHECABLEBECAMETHEFAVOUREDMEANSOFCOMMUNICATIONFORLOVERSSEPARATEDBYTHOUSANDSOFMILESITSEEMEDMIRACULOUS".substring(0, len);
        int[] key = {51, 23, 15, 62, 14, 22, 39, 21, 99, 12, 19, 24, 4, 73, 6, 18, 85, 20, 11, 25, 42, 38, 8, 26, 9};
        int offset = 6;

        SolveM138.solveM138Challenge(sweepOffset ? 0 : offset, challengeString, withCrib ? crib : "", 0, key, offset);
    }

    /**
     * Klaus Schemh challenge 4
     * @param sweepOffset - if true, test all possible offsets. If false, test only the correct offset.
     * @param withCrib - if true, find the key matching the real plaintext.
     */
    private static void challenge4M138(boolean sweepOffset, boolean withCrib) {
        String challengeString = "PTIJJHDJPKYTMTKUVEPDHYKLHDEYMGLIJLNWKXVGZILQNCJRHWJNBJFUAQHNBJGXWZBESXNXPZH";
        String crib = "CRYPTOGRAPHYPROVIDESMEANSFORSECURECOMMUNICATIONSINTHEPRESENCEOFTHIRDPARTIES";
        int[] key = {79, 62, 66, 12, 18, 88, 27, 54, 91, 85, 72, 90, 76, 78, 36, 28, 30, 41, 48, 2, 8, 22, 59, 98, 33};
        int offset = 22;
        //                 79,42!,66,12,18,88,27,54,91,60!,72,90,76,78,36,28,30,41,48,02,08,22,59,98,33

        SolveM138.solveM138Challenge(sweepOffset ? 0 : offset, challengeString, withCrib ? crib : "", 0, key, offset);
    }

    private static void createMenuOptions() {

        MainMenuOption.createCommonMenuOptions();

        MainMenuOption.add(new MainMenuOption(
                Flag.MODEL,
                "m",
                "Model",
                "Multiplex system model. M138 or M94.",
                false,
                "M94"));

        MainMenuOption.add(new MainMenuOption(
                Flag.OFFSET,
                "o",
                "Offset(s)",
                "Known offset(s), from 1 to 25. If offset(s) is(are) unknown, will look for all possible offsets.",
                false,
                1, 25));

        MainMenuOption.add(new MainMenuOption(
                Flag.SIMULATION,
                "s",
                "Simulation",
                "Create ciphertext from random key. Simulation modes: 0 (default) - no simulation, 1 - offset(s) unknown, 2 - - offset(s) known.",
                false,
                0, 2, 0));

        MainMenuOption.add(new MainMenuOption(
                Flag.SIMULATION_TEXT_LENGTH,
                "l",
                "Length of text for simulation",
                "Length of random plaintext encrypted for simulation.",
                false,
                1, 1000, 75));

    }

    public static void main(String[] args) {

        createMenuOptions();
        //MainMenuOption.printUsage();

        CtAPI.open("Multiplex", "1.0");

        String[] ctArgs = CtAPI.getArgs();
        if (ctArgs != null && ctArgs.length > 0) {
            if (!MainMenuOption.parseAllOptions(ctArgs, false)) {
                MainMenuOption.printUsage();
                return;
            }
        }
        if (!MainMenuOption.parseAllOptions(args, true)) {
            MainMenuOption.printUsage();
            return;
        }

        MainMenuOption.printOptions();

        final ArrayList<Integer> OFFSET = MainMenuOption.getIntegerValues(Flag.OFFSET);
        final String MODEL = MainMenuOption.getStringValue(Flag.MODEL);
        final boolean M138 = MODEL.contains("138");
        final String RESOURCE_PATH = MainMenuOption.getStringValue(Flag.RESOURCE_PATH);
        final int SIMULATION = MainMenuOption.getIntegerValue(Flag.SIMULATION);
        final int CYCLES = MainMenuOption.getIntegerValue(Flag.CYCLES);
        String CIPHERTEXT = MainMenuOption.getStringValue(Flag.CIPHERTEXT);
        if (CIPHERTEXT.endsWith("txt")) {
            CIPHERTEXT = Utils.readCipherFile(CIPHERTEXT);
        }
        final String CRIB = MainMenuOption.getStringValue(Flag.CRIB);
        final int SIMULATION_TEXT_LENGTH = MainMenuOption.getIntegerValue(Flag.SIMULATION_TEXT_LENGTH);
        final int THREADS = MainMenuOption.getIntegerValue(Flag.THREADS);

        if (!Stats.readHexaStatsFile(RESOURCE_PATH + "/" + Utils.HEXA_FILE)) {
            CtAPI.goodbye(-1, "Could not read hexa file .... " + RESOURCE_PATH + "/" + Utils.HEXA_FILE);
        }

        /*
        String[] mauborgne = {"VFDJL QMMJB HSYVJ KCJTJ WDKNI".replaceAll(" ",""),
                "CGNJM ZVKQC JPRJR CGOXG UCZVC ".replaceAll(" ",""),
                "CSTDT SSDJN JDKKT IXVEX VHDVK ".replaceAll(" ",""),
                "OZBGF VTUEC UGTZD KYWJR VZSDG ".replaceAll(" ",""),
                "QIRMB FTKBY CGAQV DQCVQ AHZGY ".replaceAll(" ",""),
                "VQWRM IHDHB RQBWU LKJCS KEYUU ".replaceAll(" ",""),
                "SSEIQ DWHNH QHGIK HAADN GNFBY ".replaceAll(" ",""),
                "VXDVX NIGJO PCOTN GKWAX YTNWL ".replaceAll(" ",""),
                "QJRLH AWTWU CYXVM BGJCR SBHWF ".replaceAll(" ",""),
                "DULPK UXMVL XFUPS ULRZK PDALY ".replaceAll(" ",""),
                "DCAIY LUPMB NACQE OPTLH KKRGT ".replaceAll(" ",""),
                "MGODT VGUYX NHKBE WPOUR VTQOE ".replaceAll(" ",""),
                "TBVEB QDXGP LCPUY AVVBK ZEOZY ".replaceAll(" ",""),
                "FIJDW WBKTY GBSMB PZWYP RRZCW ".replaceAll(" ",""),
                "DYVPJ CLNXE SCMF0 YPIZF PEBHM ".replaceAll(" ",""),
                "MYYTJ RFMEP PHDXP ODFZO WLGLA ".replaceAll(" ",""),
                "EYKKD XHTEV TRXWK CJPSG MASCY ".replaceAll(" ",""),
                "LGQLV HTUIP YAUGJ PGDLH UZTKV ".replaceAll(" ",""),
                "BRKTJ RGGTB HMLXX FRHOA AZVWU ".replaceAll(" ",""),
                "CDUDV DBZUA ELRPO SPUJD XRZWA ".replaceAll(" ",""),
                "EUFBT TWNIY HHTNW QNFVE NYGBY ".replaceAll(" ",""),
                "TUTVY NGLPG TYOLI HXZQT XSGOJ ".replaceAll(" ",""),
                "PBTJC CJONJ UNIXB UAQBI WNIHL ".replaceAll(" ",""),
                "VHNKR XVZMD KFHUY XRNDD KXXVM ".replaceAll(" ",""),
                "NNHBF VQH0B LXCYM AKFLS SSJXG".replaceAll(" ","")};
        String plotz1M94 = "JUTHGFFHJTEUONGWZLIZAGOPIILLGZWCYPQNDZNICSWEILYSUALYRMEGKBUPUZCOSBCPIMSMRDW";


        challengeM94(mauborgne[2] + mauborgne[6] + mauborgne[4], 3);
        challengeM94(plotz1M94, 1);
        challenge3M138(125, true, false);
        challenge4M138(true, false);

        */
        if ((CIPHERTEXT == null || CIPHERTEXT.isEmpty()) && SIMULATION == 0) {
            CtAPI.goodbye(-1, "Ciphertext or ciphertext file required when not in simulation mode\n");
        }
        if (M138) {
            CtAPI.printf("Starting search ... might take a few minutes\n");
            if (SIMULATION == 0) {
                SolveM138.solveM138Challenge(OFFSET.isEmpty()? 0 : OFFSET.get(0), CIPHERTEXT, CRIB, CYCLES, null, 0);
            } else {
                simulatedM138(RESOURCE_PATH + "/" + Utils.BOOK_FILE, SIMULATION_TEXT_LENGTH, SIMULATION == 1, false);
            }
        } else {
            if (SIMULATION == 0) {
                if (OFFSET.size() == 0) {
                    CtAPI.printf("Starting search with %d threads ... might take a few minutes\n", THREADS);
                    challengeM94(CIPHERTEXT, CRIB, CYCLES, 3, THREADS);
                } else {
                    CtAPI.printf("Starting search ... might take a few minutes\n");
                    challengeM94KnownOffsets(CIPHERTEXT, CRIB, CYCLES, OFFSET);
                }
            } else {
                CtAPI.printf("Starting search with %d threads ... might take a few minutes\n", THREADS);
                simulatedM94(RESOURCE_PATH + "/" + Utils.BOOK_FILE, SIMULATION_TEXT_LENGTH, SIMULATION == 1, CYCLES, THREADS);
            }
        }

        CtAPI.goodbye();
    }
}
