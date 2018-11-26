package m209;

import common.*;

public class Main {
    private static void createCommandLineArguments() {

        CommandLine.createCommonArguments();

        CommandLine.add(new CommandLineArgument(
                Flag.LANGUAGE,
                "g",
                "Language",
                "Language used for statistics and for simulation random text.",
                false,
                "ENGLISH",
                new String[]{"ENGLISH", "FRENCH", "ITALIAN", "GERMAN",}));

        CommandLine.add(new CommandLineArgument(
                Flag.VERSION,
                "y",
                "Version",
                "Version of operating instructions.",
                false,
                "V1947",
                new String[]{"V1942", "V1944", "V1947", "V1953", "UNRESTRICTED",}));

        CommandLine.add(new CommandLineArgument(
                Flag.SIMULATION,
                "s",
                "Simulation",
                "Create ciphertext from random key and plaintext. Simulation modes: 0 (default) - no simulation, 1 - ciphertext only, 2 - with crib.",
                false,
                0, 2, 0));

        CommandLine.add(new CommandLineArgument(
                Flag.SIMULATION_TEXT_LENGTH,
                "l",
                "Length of text for simulation",
                "Length of random plaintext encrypted for simulation.",
                false,
                1, 5000, 1500));

        CommandLine.add(new CommandLineArgument(
                Flag.SIMULATION_OVERLAPS,
                "o",
                "Number of lug overlaps for simulation key",
                "Number of lug overlaps for simulation key i.e. bars involving two wheels.",
                false,
                0, 14, 2));
    }

    public static void main(String[] args) {

        createCommandLineArguments();
        BestResults.setDiscardSamePlaintexts(false);
        BestResults.setThrottle(true);
        //CommandLineArgument.printUsage();

        CtAPI.open("M209 attacks", "1.0");

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
        final int CYCLES = CommandLine.getIntegerValue(Flag.CYCLES);
        final int THREADS = CommandLine.getIntegerValue(Flag.THREADS);
        final String CRIB = CommandLine.getStringValue(Flag.CRIB);
        String CIPHERTEXT = CommandLine.getStringValue(Flag.CIPHERTEXT);
        if (CIPHERTEXT.endsWith("txt")) {
            CIPHERTEXT = Utils.readTextFile(CIPHERTEXT);
        }

        final int SIMULATION = CommandLine.getIntegerValue(Flag.SIMULATION);
        final int SIMULATION_TEXT_LENGTH = CommandLine.getIntegerValue(Flag.SIMULATION_TEXT_LENGTH);
        final int SIMULATION_OVERLAPS = CommandLine.getIntegerValue(Flag.SIMULATION_OVERLAPS);
        final Version VERSION = Version.valueOf(CommandLine.getStringValue(Flag.VERSION));
        final Language LANGUAGE = Language.valueOf(CommandLine.getStringValue(Flag.LANGUAGE));

        Global.initVersion(VERSION);

        if (SIMULATION != 0) {
            ReportResult.simulation = true;
            Key simulationKey = Simulation.simulation(RESOURCE_PATH, LANGUAGE, SIMULATION_TEXT_LENGTH, SIMULATION_OVERLAPS);
            if (SIMULATION == 2) {
                KP.solveMultithreaded(simulationKey.cipher, simulationKey.crib, simulationKey, CYCLES, THREADS);
            } else {
                CO.solveMultithreaded(RESOURCE_PATH, LANGUAGE, simulationKey.cipher, simulationKey, CYCLES, THREADS);
            }
        } else {
            if (CIPHERTEXT == null || CIPHERTEXT.isEmpty()) {
                CtAPI.goodbye(-1, "Ciphertext or ciphertext file required when not in simulation mode\n");
            }
            if (CRIB != null && !CRIB.isEmpty()) {
                KP.solveMultithreaded(CIPHERTEXT, CRIB, null, CYCLES, THREADS);
            } else {
                CO.solveMultithreaded(RESOURCE_PATH, LANGUAGE, CIPHERTEXT, null, CYCLES, THREADS);
            }
        }

        CtAPI.goodbye();
    }

}
