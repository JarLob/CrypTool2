package common;

import org.cryptool.ipc.Ct2Connector;
import org.cryptool.ipc.messages.Ct2IpcMessages;

import java.util.HashMap;
import java.util.Map;

public class CtAPI {

    private static int INPUT_CIPHERTEXT = 1;
    private static int INPUT_CRIB = 2;
    private static int INPUT_ARGS = 3;
    private static int INPUT_THREADS = 100;
    private static int INPUT_CYCLES = 200;
    private static int INPUT_RESOURCES = 300;
    private static int OUTPUT_PLAINTEXT = 1;
    private static int OUTPUT_KEY = 2;
    private static int OUTPUT_SCORE = 3;
    private static int OUTPUT_BEST_RESULTS = 1000;

    private static Map<Integer, String> params = new HashMap<>();

    public static String[] getArgs() {

        String args = params.get(INPUT_ARGS);
        if (args != null && args.trim().length() > 0) {
            println("Received remote args:" + args);
        } else {
            args = "";
        }
        String ciphertext = params.get(INPUT_CIPHERTEXT);
        if (ciphertext != null && ciphertext.trim().length() > 0) {
            args += " -i " + ciphertext;
            println("Received remote ciphertext: " + ciphertext);
        }
        String crib = params.get(INPUT_CRIB);
        if (crib != null && crib.trim().length() > 0) {
            args += " -p " + crib;
            println("Received remote crib: " + crib);
        }

        String resourcePath = params.get(INPUT_RESOURCES);
        if (resourcePath != null && resourcePath.trim().length() > 0) {
            args += " -r " + resourcePath;
            println("Received resource path: " + resourcePath);
        }

        String threads = params.get(INPUT_THREADS);
        if (threads != null && threads.trim().length() > 0) {
            args += " -t " + threads;
            println("Received threads: " + threads);
        }

        String cycles = params.get(INPUT_CYCLES);
        if (cycles != null && cycles.trim().length() > 0) {
            args += " -n " + cycles;
            println("Received threads: " + cycles);
        }


        if (args.isEmpty()) {
            return new String[]{};
        }

        args = args.replaceAll("[\\n\\r]+", " ");
        args = args.replaceAll(" +", " ");
        args = args.trim();
        println("Summary of remote args: " + args);

        return args.split(" ");

    }

    public static void shutdownIfNeeded() {
        if (Ct2Connector.getShutdownRequested()) {
            println("Received request to shutdown ....");
            java.awt.Toolkit.getDefaultToolkit().beep();
            System.exit(0);
        }
    }

    public static void open(String algorithmName, String algorithmVersion) {
        long start = System.currentTimeMillis();
        int received = 0;
        String prevReceiverState = "";
        String prevSenderState = "";
        try {
            Ct2Connector.start(algorithmName, algorithmVersion, null);
            do {
                shutdownIfNeeded();
                int previousReceived = received;
                if (Ct2Connector.hasValues()) {
                    Map<Integer, String> values = Ct2Connector.getValues();
                    for (int input : values.keySet()) {
                        params.put(input, values.get(input));
                        printf("Received value for %d: %s\n", input, params.get(input));
                        received++;
                    }
                }
                String newReceiverState = Ct2Connector.getReceiverState().toString();
                String newSenderState = Ct2Connector.getSenderState().toString();
                if (!newReceiverState.equals(prevReceiverState) || !newSenderState.equals(prevSenderState) || received > previousReceived) {
                    System.out.printf("Receiver: %-15s, Sender: %-15s, Read: %d inputs\n", newReceiverState, newSenderState, received);
                    prevSenderState = newSenderState;
                    prevReceiverState = newReceiverState;
                }
            } while (System.currentTimeMillis() - start < 1000);
            if (received > 0) {
                CtAPI.println("Available processors (cores): " + Runtime.getRuntime().availableProcessors());
                CtAPI.printf("Free memory (bytes): %,d\n\n", Runtime.getRuntime().freeMemory());
            }
        } catch (Exception e) {
            displayExceptionAndGoodbye(e);
        }
    }

    private static void displayExceptionAndGoodbye(Exception e) {
        logError(e.getMessage());
        e.printStackTrace();
        goodbye(-1, e.getMessage());
    }

    public static synchronized void updateProgress(int progress, int maxValue) {
        try {
            if (maxValue <= 0) {
                updateProgress(progress % 100, 95);
            } else {
                Ct2Connector.encodeProgress(progress, maxValue);
            }
        } catch (Exception e) {
            displayExceptionAndGoodbye(e);
        }
    }

    private static void log(String s, Ct2IpcMessages.Ct2LogEntry.LogLevel level) {

        try {
            while (!s.isEmpty() && (s.charAt(s.length() - 1) == '\n' || s.charAt(s.length() - 1) == '\r')) {
                s = s.substring(0, s.length() - 1);
            }
            Ct2Connector.encodeLogEntry(s, level);
        } catch (Exception e) {
            displayExceptionAndGoodbye(e);
        }
    }

    public static void printf(String format, Object... objects) {
        String s = String.format(format, objects);
        print(s);
    }

    public static void print(String s) {
        logInfo(s);
        System.out.print(s);
    }

    public static void println(String s) {
        logInfo(s);
        System.out.println(s);
    }

    private static void logInfo(String s) {
        log(s, Ct2IpcMessages.Ct2LogEntry.LogLevel.CT2INFO);
    }

    private static void logError(String message) {
        log(message, Ct2IpcMessages.Ct2LogEntry.LogLevel.CT2ERROR);
        System.out.println("Error: " + message);
    }

    private static void updateOutput(int i, String value) {

        try {
            Map<Integer, String> valuemap = new HashMap<>();
            valuemap.put(i, value);
            Ct2Connector.enqueueValues(valuemap);
        } catch (Exception e) {
            displayExceptionAndGoodbye(e);
        }
    }

    public static void updateBestList(String bestList) {
        updateOutput(OUTPUT_BEST_RESULTS, bestList);
    }

    public static void updateBestResult(Result result) {
        updateOutput(OUTPUT_SCORE, String.format("%,d", result.score));
        updateOutput(OUTPUT_KEY, result.keyString);
        updateOutput(OUTPUT_PLAINTEXT, result.plaintextString);
        printf("Best: %,12d %s %s %s\n", result.score, result.keyString, result.plaintextString, result.commentString);
    }

    public static void updateBestResult(Result result, Result original) {
        updateOutput(OUTPUT_KEY, result.keyString + " (Original:" + original.keyString + ")");
        updateOutput(OUTPUT_SCORE, String.format("%,d (Original:%,d)", result.score, original.score));
        updateOutput(OUTPUT_PLAINTEXT, result.plaintextString + " (Original:" + original.plaintextString + ")");
        printf("Best: %,12d %s %s %s\n", result.score, result.keyString, result.plaintextString, result.commentString);
        printf("      %,12d %s %s %s\n", original.score, original.keyString, original.plaintextString, original.commentString);
    }


    public static void goodbye(int exitCode, String message) {
        if (exitCode != 0) {
            logError(String.format("Completed with error code %s (%s)- shutting down\n", exitCode, message));
        } else {
            printf(message);
        }

        long start = System.currentTimeMillis();
        while (System.currentTimeMillis() - start < 5_000) {
            try {
                Ct2Connector.encodeGoodbye(exitCode, message);
                Thread.sleep(100);
                if (Ct2Connector.getShutdownRequested()) {
                    println("Received shutdown command from CrypTool");
                    break;
                }
            } catch (InterruptedException e) {
                displayExceptionAndGoodbye(e);
            }
        }

        java.awt.Toolkit.getDefaultToolkit().beep();
        System.exit(exitCode);
    }

    public static void goodbye() {
        goodbye(0, "Shutting down ... ");
    }
}
