package playfair;

import common.*;

import java.util.Random;

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


    private static void createMenuOptions() {


        MainMenuOption.add(new MainMenuOption(
                Flag.CIPHERTEXT,
                "i",
                "Ciphertext or ciphertext file",
                "Ciphertext string, or full path for the file with the cipher, ending with .txt.",
                false,
                ""));

        MainMenuOption.add(new MainMenuOption(
                Flag.CRIB,
                "p",
                "Crib (known-plaintext)",
                "Known plaintext (crib) at the beginning of the message.",
                false,
                ""));

        MainMenuOption.add(new MainMenuOption(
                Flag.RESOURCE_PATH,
                "r",
                "Resource directory",
                "Full path of directory for resources (e.g. stats files).",
                false,
                "."));

        MainMenuOption.add(new MainMenuOption(
                Flag.THREADS,
                "t",
                "Number of processing threads",
                "Number of threads, for multithreading. 1 for no multithreading.",
                false,
                1, 15, 7));

        MainMenuOption.add(new MainMenuOption(
                Flag.CYCLES,
                "n",
                "Number of cycles",
                "Number of cycles for simulated annealing. 0 for infinite.",
                false,
                0, 1000, 0));

        MainMenuOption.add(new MainMenuOption(
                Flag.SIMULATION,
                "s",
                "Simulation",
                "Create ciphertext from random key and plaintext from book file."));

        MainMenuOption.add(new MainMenuOption(
                Flag.SIMULATION_TEXT_LENGTH,
                "l",
                "Length of text for simulation",
                "Length of random plaintext encrypted for simulation.",
                false,
                1, 1000, 100));

    }

    public static void main(String[] args) {


        createMenuOptions();
        //MainMenuOption.printUsage();

        CtAPI.open("Playfair", "1.0");

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

        final String CRIB = MainMenuOption.getStringValue(Flag.CRIB);
        final int THREADS = MainMenuOption.getIntegerValue(Flag.THREADS);
        final int CYCLES = MainMenuOption.getIntegerValue(Flag.CYCLES);
        final String RESOURCE_PATH = MainMenuOption.getStringValue(Flag.RESOURCE_PATH);
        final int SIMULATION_TEXT_LENGTH = MainMenuOption.getIntegerValue(Flag.SIMULATION_TEXT_LENGTH);
        String CIPHERTEXT = MainMenuOption.getStringValue(Flag.CIPHERTEXT);
        if (CIPHERTEXT.endsWith("txt")) {
            CIPHERTEXT = Utils.readCipherFile(CIPHERTEXT);
        }
        final boolean SIMULATION = MainMenuOption.getBooleanValue(Flag.SIMULATION);

        if (!Stats.readHexaStatsFile(RESOURCE_PATH + "/" + Utils.HEXA_FILE)) {
            CtAPI.goodbye(-1, "Could not read hexa file .... " + RESOURCE_PATH + "/" + Utils.HEXA_FILE);
        }
        Transform.printTransformationsCounts();

        Key simulationKey = null;
        final boolean debug = false;
        int[] cipherText;
        if (SIMULATION) {
            int[] plainText = new int[SIMULATION_TEXT_LENGTH];
            Utils.readPlaintextSegmentFromFile(RESOURCE_PATH + "/" + Utils.BOOK_FILE, Utils.randomGet(50000), plainText);
            Playfair.preparePlainText(plainText);
            simulationKey = new Key();
            simulationKey.random();
            cipherText = new int[SIMULATION_TEXT_LENGTH];
            if (cipherText.length % 2 != 0) {
                CtAPI.goodbye(-1, "Ciphertext length must be even - found " + cipherText.length + " characters");
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
            BestList.setOriginal(simulationKey.score, simulationKey.toString(), Utils.getString(plainText), "Original");
        } else {
            if ((CIPHERTEXT == null || CIPHERTEXT.isEmpty())) {
                CtAPI.goodbye(-1, "Ciphertext or ciphertext file required when not in simulation mode");
            }
            cipherText = Utils.getText(CIPHERTEXT);
            if (cipherText.length % 2 != 0) {
                CtAPI.goodbye(-1, "Ciphertext length must be even - found " + cipherText.length + " characters");
            }
            CtAPI.printf("Ciphertext: %s Length: %d\n", Utils.getString(cipherText), cipherText.length);
        }


        Random r = new Random();
        final Key simulationKey_ = simulationKey;
        for (int t_ = 0; t_ < THREADS; t_++) {
            final int t = t_;
            double factor = 0.5 + r.nextDouble();
            factor = 1.0;
            int multiplier = (int) (factor* 150_000)/cipherText.length;
            new Thread(
                    new Runnable() {
                        @Override
                        public void run() {
                            SolvePlayfair.solveSA(t, CYCLES, 200_000, multiplier, cipherText, CRIB, simulationKey_);
                        }
                    }
            ).start();
        }
    }


}
