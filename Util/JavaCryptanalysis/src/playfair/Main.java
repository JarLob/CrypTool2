package playfair;

import common.*;

public class Main {

        //"pp1.txt";
        //"pp2.txt";
        //"pp3.txt";  // no solution
        //"pp4.txt"; // no solution
        //"pp5.txt"; // solved
        //"pp6.txt";
        //"pp7.txt";
        //"pp8.txt";
        //"pp9.txt";
        // "pp10.txt";
        //"cowan1.txt";
        //"codebreakers.txt";
        //"task6.txt";
        //"mason1.txt";
        //"mason2.txt";
        //"mason3.txt"; //BUTNOSUCHHAPPYMARRIAGECOULDNOWTEACHTHEADMIRINGMULTITUDEWHATCONNUBIALFELICITYREALLYWAS
        //"mason4.txt";
        //"mason5.txt";
        //"mason6.txt";
        //"mason7.txt";
        //"code6.pf";
        //"cowanSA2.txt";
        //"gillogy.pf";
        //"helen153.pf";
        //"cowan1.txt";
        //"cowan2.txt";
        //"cowan3.txt";
        //"cowan4.txt";
        //"cowan5.txt";
        //"cowan6.txt";
        //"cowan7.txt";
        //"cowanDiff.txt";
        //"tc2006ma.txt";
        //"tc2006mj.txt";
        //"tc2006so.txt";
        //"tc2006nd.txt";
        //"tc2007jf.txt";
        //"tc2007ja.txt";
        //"tc2007so.txt";
        //"tc2008jf.txt";
        //"tc2008ma.txt";
        //"tc2008mj.txt";
        //"tc2008ja.txt";
        //"tc2008so.txt";
        //"tc2009so.txt";
        //"tc1989jf1.txt";
        //"g.txt";


    private static void createCommandLineArguments() {

        CommandLine.createCommonArguments();

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
                1, 1000, 100));

    }

    public static void main(String[] args) {


        createCommandLineArguments();
        BestResults.setScoreThreshold(1_800_000);
        BestResults.setDiscardSamePlaintexts(true);
        BestResults.setThrottle(false);

        //CommandLineArgument.printUsage();

        CtAPI.open("Playfair", "1.0");

        CommandLine.parseAndPrintCommandLineArgs(args);

        final String CRIB = CommandLine.getStringValue(Flag.CRIB);
        final int THREADS = CommandLine.getIntegerValue(Flag.THREADS);
        final int CYCLES = CommandLine.getIntegerValue(Flag.CYCLES);
        final String RESOURCE_PATH = CommandLine.getStringValue(Flag.RESOURCE_PATH);
        String CIPHERTEXT = CommandLine.getStringValue(Flag.CIPHERTEXT);
        if (CIPHERTEXT.endsWith("txt")) {
            CIPHERTEXT = Utils.readTextFile(CIPHERTEXT);
        }

        final boolean SIMULATION = CommandLine.getBooleanValue(Flag.SIMULATION);
        final int SIMULATION_TEXT_LENGTH = CommandLine.getIntegerValue(Flag.SIMULATION_TEXT_LENGTH);

        if (!Stats.readHexagramStatsFile(RESOURCE_PATH + "/" + Utils.HEXA_FILE)) {
            CtAPI.goodbyeError("Could not read hexa file .... " + RESOURCE_PATH + "/" + Utils.HEXA_FILE);
        }
        Transformations.printTransformationsCounts();

        Key simulationKey = null;
        final boolean debug = false;
        int[] cipherText;
        if (SIMULATION) {
            int[] plainText = new int[SIMULATION_TEXT_LENGTH];
            Utils.readTextSegmentFromFile(RESOURCE_PATH + "/" + Utils.BOOK_FILE, Utils.randomNextInt(50000), plainText);
            Playfair.preparePlainText(plainText);
            simulationKey = new Key();
            simulationKey.random();
            cipherText = new int[SIMULATION_TEXT_LENGTH];
            if (cipherText.length % 2 != 0) {
                CtAPI.goodbyeError("Ciphertext length must be even - found " + cipherText.length + " characters");
            }
            int cipherTextLengthFull = simulationKey.encrypt(plainText, cipherText);
            if (debug) {
                CtAPI.printf("Plaintext:  %s Length: %d\n", Utils.getString(plainText), plainText.length);
                CtAPI.printf("Ciphertext: %s Length: %d\n", Utils.getString(cipherText), cipherText.length, cipherTextLengthFull - cipherText.length);
                CtAPI.printf("Truncated %d characters\n", cipherTextLengthFull - cipherText.length);
            }
            int[] plainRemoveNulls = new int[SIMULATION_TEXT_LENGTH];
            int plainRemoveNullsLength = simulationKey.decrypt(cipherText, plainText, plainRemoveNulls);
            CtAPI.printf("Plaintext:  %s Length: %d\n", Utils.getString(plainRemoveNulls, plainRemoveNullsLength), plainRemoveNullsLength);
            CtAPI.printf("            %s Length: %d\n", Utils.getString(plainText), plainText.length);
            CtAPI.printf("Ciphertext: %s Length: %d\n", Utils.getString(cipherText), cipherText.length);
            simulationKey.setCipher(cipherText);
            simulationKey.eval();
            CtAPI.printf("Original score: %,d\n", simulationKey.score);
            BestResults.setOriginal(simulationKey.score, simulationKey.toString(), simulationKey.toString(), Utils.getString(plainText), "Original");
        } else {
            if ((CIPHERTEXT == null || CIPHERTEXT.isEmpty())) {
                CtAPI.goodbyeError("Ciphertext or ciphertext file required when not in simulation mode");
            }
            cipherText = Utils.getText(CIPHERTEXT);
            if (cipherText.length % 2 != 0) {
                CtAPI.goodbyeError("Ciphertext length must be even - found " + cipherText.length + " characters");
            }
            CtAPI.printf("Ciphertext: %s Length: %d\n", Utils.getString(cipherText), cipherText.length);
        }

        SolvePlayfair.solveMultithreaded(cipherText, CRIB, THREADS, CYCLES, simulationKey);

    }


}
