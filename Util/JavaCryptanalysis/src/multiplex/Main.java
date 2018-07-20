package multiplex;

import common.*;

import java.util.ArrayList;

class Main {


    private static void createCommandLineArguments() {

        CommandLineArgument.createCommonArguments();

        CommandLineArgument.add(new CommandLineArgument(
                Flag.MODEL,
                "m",
                "Model",
                "Multiplex system model. M138 or M94.",
                false,
                "M94"));

        CommandLineArgument.add(new CommandLineArgument(
                Flag.OFFSET,
                "o",
                "Offset(s)",
                "Known offset(s), from 0 to 25. If offset(s) is(are) unknown, will look for all possible offsets.",
                false,
                0, 25));

        CommandLineArgument.add(new CommandLineArgument(
                Flag.SIMULATION,
                "s",
                "Simulation",
                "Create ciphertext from random key. Simulation modes: 0 (default) - no simulation, 1 - offset(s) unknown, 2 - - offset(s) known.",
                false,
                0, 2, 0));

        CommandLineArgument.add(new CommandLineArgument(
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

        CtAPI.open("Multiplex", "1.0");

        String[] ctArgs = CtAPI.getArgs();
        if (!CommandLineArgument.parseArguments(ctArgs, false)) {
            CommandLineArgument.printUsage();
            return;
        }

        if (!CommandLineArgument.parseArguments(args, true)) {
            CommandLineArgument.printUsage();
            return;
        }

        CommandLineArgument.printArguments();

        final String RESOURCE_PATH = CommandLineArgument.getStringValue(Flag.RESOURCE_PATH);
        final int CYCLES = CommandLineArgument.getIntegerValue(Flag.CYCLES);
        final int THREADS = CommandLineArgument.getIntegerValue(Flag.THREADS);
        final String CRIB = CommandLineArgument.getStringValue(Flag.CRIB);
        String CIPHERTEXT = CommandLineArgument.getStringValue(Flag.CIPHERTEXT);
        if (CIPHERTEXT.endsWith("txt")) {
            CIPHERTEXT = Utils.readTextFile(CIPHERTEXT);
        }

        final ArrayList<Integer> OFFSET = CommandLineArgument.getIntegerValues(Flag.OFFSET);
        final boolean IS_M138 = CommandLineArgument.getStringValue(Flag.MODEL).contains("138");
        final int SIMULATION = CommandLineArgument.getIntegerValue(Flag.SIMULATION);
        final int SIMULATION_TEXT_LENGTH = CommandLineArgument.getIntegerValue(Flag.SIMULATION_TEXT_LENGTH);

        if (!Stats.readHexagramStatsFile(RESOURCE_PATH + "/" + Utils.HEXA_FILE)) {
            CtAPI.goodbye(-1, "Could not read hexa file .... " + RESOURCE_PATH + "/" + Utils.HEXA_FILE);
        }

        if ((CIPHERTEXT == null || CIPHERTEXT.isEmpty()) && SIMULATION == 0) {
            CtAPI.goodbye(-1, "Ciphertext or ciphertext file required when not in simulation mode\n");
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
