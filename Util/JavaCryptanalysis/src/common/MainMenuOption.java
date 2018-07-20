package common;

import java.util.ArrayList;

public class MainMenuOption {

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


    public MainMenuOption(Flag flag,
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

    public MainMenuOption(Flag flag,
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

    public MainMenuOption(Flag flag,
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


    public MainMenuOption(Flag flag,
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

    public static void createCommonMenuOptions() {
        add(new MainMenuOption(
                Flag.CIPHERTEXT,
                "i",
                "Ciphertext or ciphertext file",
                "Ciphertext string, or full path for the file with the cipher, ending with .txt.",
                false,
                ""));

        add(new MainMenuOption(
                Flag.CRIB,
                "p",
                "Crib (known-plaintext)",
                "Known plaintext (crib) at the beginning of the message.",
                false,
                ""));

        add(new MainMenuOption(
                Flag.RESOURCE_PATH,
                "r",
                "Resource directory",
                "Full path of directory for resources (e.g. stats files).",
                false,
                "."));

        add(new MainMenuOption(
                Flag.THREADS,
                "t",
                "Number of processing threads",
                "Number of threads, for multithreading. 1 for no multithreading.",
                false,
                1, 15, 7));

        add(new MainMenuOption(
                Flag.CYCLES,
                "n",
                "Number of cycles",
                "Number of cycles for simulated annealing. 0 for infinite.",
                false,
                0, 1000, 0));
    }

    public static int getIntegerValue(Flag flag) {
        for (MainMenuOption option : mainMenuOptions) {
            if (option.flag == flag) {
                if (option.type != Type.NUMERIC) {
                    CtAPI.goodbye(-1, "Not a numeric flag " + flag.toString());
                }
                if (option.multiple) {
                    CtAPI.goodbye(-1, "Multiple value numeric flag " + flag.toString());
                }
                if (option.integerArrayList.isEmpty()) {
                    CtAPI.goodbye(-1, "No value for numeric flag " + flag.toString());
                }
                return option.integerArrayList.get(0);
            }
        }
        CtAPI.goodbye(-1, "No such flag " + flag.toString());
        return -1;
    }

    public static ArrayList<Integer> getIntegerValues(Flag flag) {
        for (MainMenuOption option : mainMenuOptions) {
            if (option.flag == flag) {
                if (option.type != Type.NUMERIC) {
                    CtAPI.goodbye(-1, "Not a numeric flag " + flag.toString());
                }
                if (!option.multiple) {
                    CtAPI.goodbye(-1, "Single value numeric flag " + flag.toString());
                }
                return option.integerArrayList;
            }
        }
        CtAPI.goodbye(-1, "No such flag " + flag.toString());
        return null;
    }

    public static String getStringValue(Flag flag) {
        for (MainMenuOption option : mainMenuOptions) {
            if (option.flag == flag) {
                if (option.type != Type.STRING) {
                    CtAPI.goodbye(-1, "Not a string flag " + flag.toString());
                }
                if (option.multiple) {
                    CtAPI.goodbye(-1, "Multiple value string flag " + flag.toString());
                }
                if (option.stringArrayList.isEmpty()) {
                    CtAPI.goodbye(-1, "No value for string flag " + flag.toString());
                }
                return option.stringArrayList.get(0);
            }
        }
        CtAPI.goodbye(-1, "No such flag " + flag.toString());
        return null;
    }

    public static ArrayList<String> getStringValues(Flag flag) {
        for (MainMenuOption option : mainMenuOptions) {
            if (option.flag == flag) {
                if (option.type != Type.STRING) {
                    CtAPI.goodbye(-1, "Not a string flag " + flag.toString());
                }
                if (!option.multiple) {
                    CtAPI.goodbye(-1, "Single value string flag " + flag.toString());
                }
                return option.stringArrayList;
            }
        }
        CtAPI.goodbye(-1, "No such flag " + flag.toString());
        return null;
    }

    public static boolean getBooleanValue(Flag flag) {
        for (MainMenuOption option : mainMenuOptions) {
            if (option.flag == flag) {
                if (option.type != Type.BOOLEAN) {
                    CtAPI.goodbye(-1, "Not a boolean flag " + flag.toString());
                }
                return option.booleanValue;
            }
        }
        CtAPI.goodbye(-1, "No such flag " + flag.toString());
        return false;
    }

    private static ArrayList<MainMenuOption> mainMenuOptions = new ArrayList<>();

    public static void add(MainMenuOption option) {
        mainMenuOptions.add(option);
    }

    public static boolean parseAllOptions(String[] args, boolean setDefaults) {
        MainMenuOption currentOption = null;

        for (String arg : args) {
            if (arg.toUpperCase().startsWith("-V")) {
                return false;
            }
            if (arg.startsWith("-") && currentOption != null) {
                CtAPI.printf("Invalid option >%s<. Parameter missing for -%s (%s)\n",
                        arg, currentOption.flagString, currentOption.shortDesc);
                return false;
            }
            if (!arg.startsWith("-") && currentOption == null) {
                CtAPI.printf("Invalid option >%s<.\n", arg);
                return false;
            }
            if (arg.startsWith("-")) {
                currentOption = getMainOption(arg);
                if (currentOption == null) {
                    CtAPI.printf("Invalid option >%s<.\n", arg);
                    return false;
                }
                if (currentOption.type == Type.BOOLEAN) {
                    currentOption.booleanValue = true;
                    currentOption = null;
                }
                continue;
            }

            // Handle string or numeric values.
            if (currentOption.type == Type.NUMERIC) {
                Integer value = null;
                try {
                    value = Integer.valueOf(arg);
                    if (value >= currentOption.minIntValue && value <= currentOption.maxIntValue) {
                        if (!currentOption.multiple && currentOption.integerArrayList.size() > 0) {
                            CtAPI.printf("Warning: duplicate value >%s< for -%s (%s).\nPrevious value >%d< discarded.\n",
                                    arg, currentOption.flagString, currentOption.shortDesc, currentOption.integerArrayList.get(0));
                            currentOption.integerArrayList.clear();
                        }
                        currentOption.integerArrayList.add(value);
                        currentOption = null;
                        continue;
                    } else {
                        value = null;
                    }
                } catch (NumberFormatException ignored) {
                }
                if (value == null) {
                    CtAPI.printf("Invalid value >%s< for -%s (%s). \n" +
                                    "Should be between %d and %d (default is %d).\n%s\n",
                            arg, currentOption.flagString, currentOption.shortDesc,
                            currentOption.minIntValue, currentOption.maxIntValue, currentOption.defaultIntValue,
                            currentOption.longDesc);

                    return false;
                }
            }

            if (currentOption.type == Type.STRING) {
                if (currentOption.stringArrayList.size() > 0 && !currentOption.multiple) {
                    CtAPI.printf("Invalid duplicate value >%s< for -%s (%s).\nPrevious value %s discarded.\n",
                            arg, currentOption.flagString, currentOption.shortDesc, currentOption.stringArrayList.get(0));
                    currentOption.stringArrayList.clear();
                }
                currentOption.stringArrayList.add(arg);
                currentOption = null;
            }
        }

        if (currentOption != null) {
            CtAPI.printf("Parameter missing for -%s (%s)\n", currentOption.flagString, currentOption.shortDesc);
            return false;
        }

        if (setDefaults) {
            for (MainMenuOption options : mainMenuOptions) {
                if (options.type == Type.NUMERIC && options.integerArrayList.isEmpty() && !options.multiple) {
                    if (options.required) {
                        CtAPI.printf("Flag -%s is mandatory but missing (%s)\n" +
                                        "Should speficiy a value between %d and %d (default is %d).\n%s\n",
                                options.flagString, options.shortDesc, options.minIntValue,
                                options.maxIntValue, options.defaultIntValue, options.longDesc);
                        return false;
                    } else {
                        options.integerArrayList.add(options.defaultIntValue);
                    }
                } else if (options.type == Type.STRING && options.stringArrayList.isEmpty() && !options.multiple) {
                    if (options.required) {
                        CtAPI.printf("Flag -%s is mandatory but missing (%s).\n%s\n",
                                options.flagString, options.shortDesc, options.longDesc);
                        return false;
                    } else {
                        options.stringArrayList.add(options.defaultStringValue);
                    }
                }
            }
        }
        return true;

    }


    public static void printOptions() {

        CtAPI.println("Input Parameters\n");

        for (MainMenuOption options : mainMenuOptions) {

            StringBuilder s = new StringBuilder(String.format("%-80s (-%s): \t", options.shortDesc, options.flagString));

            switch (options.type) {
                case BOOLEAN:
                    s.append(" ").append(options.booleanValue);
                    break;
                case NUMERIC:

                    boolean first = true;
                    for (Integer numericValue : options.integerArrayList) {
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
                    for (String stringValue : options.stringArrayList) {
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

    private static MainMenuOption getMainOption(String arg) {
        MainMenuOption currentOption = null;
        for (MainMenuOption mainMenuOption : mainMenuOptions) {
            if (mainMenuOption.flagString.equalsIgnoreCase(arg.substring(1))) {
                currentOption = mainMenuOption;
                break;
            }
        }
        return currentOption;
    }

    public static void printUsage() {

        CtAPI.print("\nUsage: java -jar <jarname>.jar [options]\nOptions:\n");

        for (MainMenuOption currentOption : mainMenuOptions) {

            String prefix = String.format("\t-%s \t%s", currentOption.flagString, currentOption.shortDesc);

            switch (currentOption.type) {
                case BOOLEAN:
                    if (currentOption.required && currentOption.multiple) {
                        CtAPI.printf("%s  (required, one or more).\n\t\t%s\n", prefix, currentOption.longDesc);
                    } else if (currentOption.required) {
                        CtAPI.printf("%s  (required).\n\t\t%s\n", currentOption.longDesc);
                    } else if (currentOption.multiple) {
                        CtAPI.printf("%s  (optional, one or more).\n\t\t%s\n", prefix, currentOption.longDesc);
                    } else {
                        CtAPI.printf("%s  (optional).\n\t\t%s\n", prefix, currentOption.longDesc);
                    }
                    break;
                case NUMERIC:
                    if (currentOption.required && currentOption.multiple) {
                        CtAPI.printf("%s \n\t\tShould specify a value between %d and %d (required, one or more).\n\t\t%s\n",
                                prefix,
                                currentOption.minIntValue,
                                currentOption.maxIntValue,
                                currentOption.longDesc);
                    } else if (currentOption.required) {
                        CtAPI.printf("%s \n\t\tShould specify a value between %d and %d (required).\n\t\t%s\n",
                                prefix,
                                currentOption.minIntValue,
                                currentOption.maxIntValue,
                                currentOption.longDesc);
                    } else if (currentOption.multiple) {
                        CtAPI.printf("%s \n\t\tShould specify a value between %d and %d (optional, one or more).\n\t\t%s\n",
                                prefix,
                                currentOption.minIntValue, currentOption.maxIntValue,
                                currentOption.longDesc);
                    } else {
                        CtAPI.printf("%s \n\t\tShould specify a value between %d and %d (optional, default is %d).\n\t\t%s\n",
                                prefix,
                                currentOption.minIntValue, currentOption.maxIntValue,
                                currentOption.defaultIntValue,
                                currentOption.longDesc);
                    }
                    break;
                case STRING:
                    if (currentOption.required && currentOption.multiple) {
                        CtAPI.printf("%s  (required, one or more).\n\t\t%s\n", prefix, currentOption.longDesc);
                    } else if (currentOption.required) {
                        CtAPI.printf("%s  (required).\n\t\t%s\n", prefix, currentOption.longDesc);
                    } else if (currentOption.multiple) {
                        CtAPI.printf("%s  (optional, one or more).\n\t\t%s\n", prefix, currentOption.longDesc);
                    } else {
                        CtAPI.printf("%s  (optional, default is \"%s\").\n\t\t%s\n",
                                prefix,
                                currentOption.defaultStringValue,
                                currentOption.longDesc);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
