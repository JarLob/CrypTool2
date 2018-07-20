package common;

import java.util.ArrayList;

public class CommandLineArgument {

    enum Type {BOOLEAN, NUMERIC, STRING}

    private Flag flag;
    private String flagString;
    private Type type;
    private String shortDesc;
    private String longDesc;

    private boolean required;
    private boolean multiple;

    private int minIntValue;
    private int maxIntValue;
    private int defaultIntValue;

    private String defaultStringValue = "";

    // Values
    private boolean booleanValue = false;
    private ArrayList<Integer> integerArrayList = new ArrayList<>();
    private ArrayList<String> stringArrayList = new ArrayList<>();

    // Long desc;


    public CommandLineArgument(Flag flag,
                               String flagString,
                               String shortDesc,
                               String longDesc) {
        this.flag = flag;
        this.flagString = flagString;
        this.type = Type.BOOLEAN;
        this.longDesc = longDesc;
        this.shortDesc = shortDesc;

        this.required = false;
        this.multiple = false;

    }

    public CommandLineArgument(Flag flag,
                               String flagString,
                               String shortDesc,
                               String longDesc,

                               boolean required,

                               int minIntValue,
                               int maxIntValue,
                               int defaultIntValue) {
        this.flag = flag;
        this.flagString = flagString;
        this.type = Type.NUMERIC;
        this.longDesc = longDesc;
        this.shortDesc = shortDesc;

        this.required = required;
        this.multiple = false;

        this.minIntValue = minIntValue;
        this.maxIntValue = maxIntValue;
        this.defaultIntValue = defaultIntValue;

    }

    public CommandLineArgument(Flag flag,
                               String flagString,
                               String shortDesc,
                               String longDesc,

                               boolean required,

                               int minIntValue,
                               int maxIntValue) {
        this.flag = flag;
        this.flagString = flagString;
        this.type = Type.NUMERIC;
        this.longDesc = longDesc;
        this.shortDesc = shortDesc;

        this.required = required;
        this.multiple = true;

        this.minIntValue = minIntValue;
        this.maxIntValue = maxIntValue;
    }


    public CommandLineArgument(Flag flag,
                               String flagString,
                               String shortDesc,
                               String longDesc,

                               boolean required,

                               String defaultStringValue) {
        this.flag = flag;
        this.flagString = flagString;
        this.type = Type.STRING;
        this.longDesc = longDesc;
        this.shortDesc = shortDesc;

        this.required = required;
        this.multiple = false;

        this.defaultStringValue = defaultStringValue;


    }

    public static void createCommonArguments() {
        add(new CommandLineArgument(
                Flag.CIPHERTEXT,
                "i",
                "Ciphertext or ciphertext file",
                "Ciphertext string, or full path for the file with the cipher, ending with .txt.",
                false,
                ""));

        add(new CommandLineArgument(
                Flag.CRIB,
                "p",
                "Crib (known-plaintext)",
                "Known plaintext (crib) at the beginning of the message.",
                false,
                ""));

        add(new CommandLineArgument(
                Flag.RESOURCE_PATH,
                "r",
                "Resource directory",
                "Full path of directory for resources (e.g. stats files).",
                false,
                "."));

        add(new CommandLineArgument(
                Flag.THREADS,
                "t",
                "Number of processing threads",
                "Number of threads, for multithreading. 1 for no multithreading.",
                false,
                1, 15, 7));

        add(new CommandLineArgument(
                Flag.CYCLES,
                "n",
                "Number of cycles",
                "Number of cycles for simulated annealing. 0 for infinite.",
                false,
                0, 1000, 0));
    }

    public static int getIntegerValue(Flag flag) {
        for (CommandLineArgument argument : arguments) {
            if (argument.flag == flag) {
                if (argument.type != Type.NUMERIC) {
                    CtAPI.goodbye(-1, "Not a numeric flag " + flag.toString());
                }
                if (argument.multiple) {
                    CtAPI.goodbye(-1, "Multiple value numeric flag " + flag.toString());
                }
                if (argument.integerArrayList.isEmpty()) {
                    CtAPI.goodbye(-1, "No value for numeric flag " + flag.toString());
                }
                return argument.integerArrayList.get(0);
            }
        }
        CtAPI.goodbye(-1, "No such flag " + flag.toString());
        return -1;
    }

    public static ArrayList<Integer> getIntegerValues(Flag flag) {
        for (CommandLineArgument argument : arguments) {
            if (argument.flag == flag) {
                if (argument.type != Type.NUMERIC) {
                    CtAPI.goodbye(-1, "Not a numeric flag " + flag.toString());
                }
                if (!argument.multiple) {
                    CtAPI.goodbye(-1, "Single value numeric flag " + flag.toString());
                }
                return argument.integerArrayList;
            }
        }
        CtAPI.goodbye(-1, "No such flag " + flag.toString());
        return null;
    }

    public static String getStringValue(Flag flag) {
        for (CommandLineArgument argument : arguments) {
            if (argument.flag == flag) {
                if (argument.type != Type.STRING) {
                    CtAPI.goodbye(-1, "Not a string flag " + flag.toString());
                }
                if (argument.multiple) {
                    CtAPI.goodbye(-1, "Multiple value string flag " + flag.toString());
                }
                if (argument.stringArrayList.isEmpty()) {
                    CtAPI.goodbye(-1, "No value for string flag " + flag.toString());
                }
                return argument.stringArrayList.get(0);
            }
        }
        CtAPI.goodbye(-1, "No such flag " + flag.toString());
        return null;
    }

    public static ArrayList<String> getStringValues(Flag flag) {
        for (CommandLineArgument argument : arguments) {
            if (argument.flag == flag) {
                if (argument.type != Type.STRING) {
                    CtAPI.goodbye(-1, "Not a string flag " + flag.toString());
                }
                if (!argument.multiple) {
                    CtAPI.goodbye(-1, "Single value string flag " + flag.toString());
                }
                return argument.stringArrayList;
            }
        }
        CtAPI.goodbye(-1, "No such flag " + flag.toString());
        return null;
    }

    public static boolean getBooleanValue(Flag flag) {
        for (CommandLineArgument argument : arguments) {
            if (argument.flag == flag) {
                if (argument.type != Type.BOOLEAN) {
                    CtAPI.goodbye(-1, "Not a boolean flag " + flag.toString());
                }
                return argument.booleanValue;
            }
        }
        CtAPI.goodbye(-1, "No such flag " + flag.toString());
        return false;
    }

    private static ArrayList<CommandLineArgument> arguments = new ArrayList<>();

    public static void add(CommandLineArgument argument) {
        arguments.add(argument);
    }

    public static boolean parseArguments(String[] args, boolean setDefaults) {
        CommandLineArgument currentArgument = null;

        for (String arg : args) {
            if (arg.toUpperCase().startsWith("-V")) {
                return false;
            }
            if (arg.startsWith("-") && currentArgument != null) {
                CtAPI.printf("Invalid argument >%s<. Parameter missing for -%s (%s)\n",
                        arg, currentArgument.flagString, currentArgument.shortDesc);
                return false;
            }
            if (!arg.startsWith("-") && currentArgument == null) {
                CtAPI.printf("Invalid argument >%s<.\n", arg);
                return false;
            }
            if (arg.startsWith("-")) {
                currentArgument = getMainArgument(arg);
                if (currentArgument == null) {
                    CtAPI.printf("Invalid argument >%s<.\n", arg);
                    return false;
                }
                if (currentArgument.type == Type.BOOLEAN) {
                    currentArgument.booleanValue = true;
                    currentArgument = null;
                }
                continue;
            }

            // Handle string or numeric values.
            if (currentArgument.type == Type.NUMERIC) {
                Integer value = null;
                try {
                    value = Integer.valueOf(arg);
                    if (value >= currentArgument.minIntValue && value <= currentArgument.maxIntValue) {
                        if (!currentArgument.multiple && currentArgument.integerArrayList.size() > 0) {
                            CtAPI.printf("Warning: duplicate value >%s< for -%s (%s).\nPrevious value >%d< discarded.\n",
                                    arg, currentArgument.flagString, currentArgument.shortDesc, currentArgument.integerArrayList.get(0));
                            currentArgument.integerArrayList.clear();
                        }
                        currentArgument.integerArrayList.add(value);
                        currentArgument = null;
                        continue;
                    } else {
                        value = null;
                    }
                } catch (NumberFormatException ignored) {
                }
                if (value == null) {
                    CtAPI.printf("Invalid value >%s< for -%s (%s). \n" +
                                    "Should be between %d and %d (default is %d).\n%s\n",
                            arg, currentArgument.flagString, currentArgument.shortDesc,
                            currentArgument.minIntValue, currentArgument.maxIntValue, currentArgument.defaultIntValue,
                            currentArgument.longDesc);

                    return false;
                }
            }

            if (currentArgument.type == Type.STRING) {
                if (currentArgument.stringArrayList.size() > 0 && !currentArgument.multiple) {
                    CtAPI.printf("Invalid duplicate value >%s< for -%s (%s).\nPrevious value %s discarded.\n",
                            arg, currentArgument.flagString, currentArgument.shortDesc, currentArgument.stringArrayList.get(0));
                    currentArgument.stringArrayList.clear();
                }
                currentArgument.stringArrayList.add(arg);
                currentArgument = null;
            }
        }

        if (currentArgument != null) {
            CtAPI.printf("Parameter missing for -%s (%s)\n", currentArgument.flagString, currentArgument.shortDesc);
            return false;
        }

        if (setDefaults) {
            for (CommandLineArgument arguments : arguments) {
                if (arguments.type == Type.NUMERIC && arguments.integerArrayList.isEmpty() && !arguments.multiple) {
                    if (arguments.required) {
                        CtAPI.printf("Flag -%s is mandatory but missing (%s)\n" +
                                        "Should speficiy a value between %d and %d (default is %d).\n%s\n",
                                arguments.flagString, arguments.shortDesc, arguments.minIntValue,
                                arguments.maxIntValue, arguments.defaultIntValue, arguments.longDesc);
                        return false;
                    } else {
                        arguments.integerArrayList.add(arguments.defaultIntValue);
                    }
                } else if (arguments.type == Type.STRING && arguments.stringArrayList.isEmpty() && !arguments.multiple) {
                    if (arguments.required) {
                        CtAPI.printf("Flag -%s is mandatory but missing (%s).\n%s\n",
                                arguments.flagString, arguments.shortDesc, arguments.longDesc);
                        return false;
                    } else {
                        arguments.stringArrayList.add(arguments.defaultStringValue);
                    }
                }
            }
        }
        return true;

    }


    public static void printArguments() {

        CtAPI.println("Input Parameters\n");

        for (CommandLineArgument arguments : arguments) {

            StringBuilder s = new StringBuilder(String.format("%-80s (-%s): \t", arguments.shortDesc, arguments.flagString));

            switch (arguments.type) {
                case BOOLEAN:
                    s.append(" ").append(arguments.booleanValue);
                    break;
                case NUMERIC:

                    boolean first = true;
                    for (Integer numericValue : arguments.integerArrayList) {
                        if (first) {
                            first = false;
                            s.append(" ").append(numericValue);
                        } else {
                            s.append(", ").append(numericValue);
                        }
                    }

                    break;
                case STRING:

                    first = true;
                    for (String stringValue : arguments.stringArrayList) {
                        if (first) {
                            first = false;
                            s.append(" ").append(stringValue);
                        } else {
                            s.append(", ").append(stringValue);
                        }
                    }

                    break;
                default:
                    break;
            }
            CtAPI.println(s.toString());

        }
        CtAPI.println("");

    }

    private static CommandLineArgument getMainArgument(String arg) {
        CommandLineArgument currentArgument = null;
        for (CommandLineArgument mainMenuArgument : arguments) {
            if (mainMenuArgument.flagString.equalsIgnoreCase(arg.substring(1))) {
                currentArgument = mainMenuArgument;
                break;
            }
        }
        return currentArgument;
    }

    public static void printUsage() {

        CtAPI.print("\nUsage: java -jar <jarname>.jar [arguments]\nArguments:\n");

        for (CommandLineArgument currentArgument : arguments) {

            String prefix = String.format("\t-%s \t%s", currentArgument.flagString, currentArgument.shortDesc);

            switch (currentArgument.type) {
                case BOOLEAN:
                    if (currentArgument.required && currentArgument.multiple) {
                        CtAPI.printf("%s  (required, one or more).\n\t\t%s\n", prefix, currentArgument.longDesc);
                    } else if (currentArgument.required) {
                        CtAPI.printf("%s  (required).\n\t\t%s\n", currentArgument.longDesc);
                    } else if (currentArgument.multiple) {
                        CtAPI.printf("%s  (argumental, one or more).\n\t\t%s\n", prefix, currentArgument.longDesc);
                    } else {
                        CtAPI.printf("%s  (argumental).\n\t\t%s\n", prefix, currentArgument.longDesc);
                    }
                    break;
                case NUMERIC:
                    if (currentArgument.required && currentArgument.multiple) {
                        CtAPI.printf("%s \n\t\tShould specify a value between %d and %d (required, one or more).\n\t\t%s\n",
                                prefix,
                                currentArgument.minIntValue,
                                currentArgument.maxIntValue,
                                currentArgument.longDesc);
                    } else if (currentArgument.required) {
                        CtAPI.printf("%s \n\t\tShould specify a value between %d and %d (required).\n\t\t%s\n",
                                prefix,
                                currentArgument.minIntValue,
                                currentArgument.maxIntValue,
                                currentArgument.longDesc);
                    } else if (currentArgument.multiple) {
                        CtAPI.printf("%s \n\t\tShould specify a value between %d and %d (argumental, one or more).\n\t\t%s\n",
                                prefix,
                                currentArgument.minIntValue, currentArgument.maxIntValue,
                                currentArgument.longDesc);
                    } else {
                        CtAPI.printf("%s \n\t\tShould specify a value between %d and %d (argumental, default is %d).\n\t\t%s\n",
                                prefix,
                                currentArgument.minIntValue, currentArgument.maxIntValue,
                                currentArgument.defaultIntValue,
                                currentArgument.longDesc);
                    }
                    break;
                case STRING:
                    if (currentArgument.required && currentArgument.multiple) {
                        CtAPI.printf("%s  (required, one or more).\n\t\t%s\n", prefix, currentArgument.longDesc);
                    } else if (currentArgument.required) {
                        CtAPI.printf("%s  (required).\n\t\t%s\n", prefix, currentArgument.longDesc);
                    } else if (currentArgument.multiple) {
                        CtAPI.printf("%s  (argumental, one or more).\n\t\t%s\n", prefix, currentArgument.longDesc);
                    } else {
                        CtAPI.printf("%s  (argumental, default is \"%s\").\n\t\t%s\n",
                                prefix,
                                currentArgument.defaultStringValue,
                                currentArgument.longDesc);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
