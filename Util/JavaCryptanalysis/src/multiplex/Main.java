package multiplex;

import common.*;

import java.util.ArrayList;

class Main {


    private static void createCommandLineArguments() {

        CommandLine.createCommonArguments();

        CommandLine.add(new CommandLineArgument(
                Flag.MODEL,
                "m",
                "Model",
                "Multiplex system model. M138 or M94.",
                false,
                "M94"));

        CommandLine.add(new CommandLineArgument(
                Flag.OFFSET,
                "o",
                "Offset(s)",
                "Known offset(s), from 0 to 25. If offset(s) is(are) unknown, will look for all possible offsets.",
                false,
                0, 25));

        CommandLine.add(new CommandLineArgument(
                Flag.SIMULATION,
                "s",
                "Simulation",
                "Create ciphertext from random key. Simulation modes: 0 (default) - no simulation, 1 - offset(s) unknown, 2 - - offset(s) known.",
                false,
                0, 2, 0));

        CommandLine.add(new CommandLineArgument(
                Flag.SIMULATION_TEXT_LENGTH,
                "l",
                "Length of text for simulation",
                "Length of random plaintext encrypted for simulation.",
                false,
                1, 1000, 75));

    }

    public static void main(String[] args) {

        createCommandLineArguments();
        //CommandLineArgument.printUsage();
        BestResults.setScoreThreshold(1_800_000);
        BestResults.setDiscardSamePlaintexts(true);
        BestResults.setThrottle(false);

        CtAPI.open("Multiplex", "1.0");
        BestResults.setScoreThreshold(1_800_000);

        CommandLine.parseAndPrintCommandLineArgs(args);


        final String RESOURCE_PATH = CommandLine.getStringValue(Flag.RESOURCE_PATH);
        final int CYCLES = CommandLine.getIntegerValue(Flag.CYCLES);
        final int THREADS = CommandLine.getIntegerValue(Flag.THREADS);
        final String CRIB = CommandLine.getStringValue(Flag.CRIB);
        String CIPHERTEXT = CommandLine.getStringValue(Flag.CIPHERTEXT);
        if (CIPHERTEXT.endsWith("txt")) {
            CIPHERTEXT = Utils.readTextFile(CIPHERTEXT);
        }

        final ArrayList<Integer> OFFSET = CommandLine.getIntegerValues(Flag.OFFSET);
        final boolean IS_M138 = CommandLine.getStringValue(Flag.MODEL).contains("138");
        final int SIMULATION = CommandLine.getIntegerValue(Flag.SIMULATION);
        final int SIMULATION_TEXT_LENGTH = CommandLine.getIntegerValue(Flag.SIMULATION_TEXT_LENGTH);

        if (!Stats.readHexagramStatsFile(RESOURCE_PATH + "/" + Utils.HEXA_FILE)) {
            CtAPI.goodbyeError("Could not read hexa file .... " + RESOURCE_PATH + "/" + Utils.HEXA_FILE);
        }

        if ((CIPHERTEXT == null || CIPHERTEXT.isEmpty()) && SIMULATION == 0) {
            CtAPI.goodbyeError("Ciphertext or ciphertext file required when not in simulation mode\n");
        }

        if (IS_M138) {
            CtAPI.printf("Starting search ... might take a few minutes\n");
            if (SIMULATION == 0) {
                SolveM138.solve(OFFSET, CIPHERTEXT, CRIB, CYCLES, -1);
            } else {
                SolveM138.solveSimulation(RESOURCE_PATH + "/" + Utils.BOOK_FILE, SIMULATION_TEXT_LENGTH, SIMULATION == 1, false);
            }
        } else {
            if (SIMULATION == 0) {
                if (OFFSET.isEmpty()) {
                    CtAPI.printf("Starting search with %d threads ... might take a few minutes\n", THREADS);
                    SolveM94.solveUnknownOffsets(CIPHERTEXT, CRIB, CYCLES, 3, THREADS);
                } else {
                    CtAPI.printf("Starting search ... might take a few minutes\n");
                    SolveM94.solveKnownOffsets(CIPHERTEXT, CRIB, CYCLES, OFFSET);
                }
            } else {
                CtAPI.printf("Starting search with %d threads ... might take a few minutes\n", THREADS);
                SolveM94.solveSimulation(RESOURCE_PATH + "/" + Utils.BOOK_FILE, SIMULATION_TEXT_LENGTH, SIMULATION == 1, CYCLES, THREADS);
            }
        }

        CtAPI.goodbye();
    }
}
