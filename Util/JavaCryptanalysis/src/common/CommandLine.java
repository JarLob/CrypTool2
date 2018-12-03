package common;

import java.util.ArrayList;

public class CommandLine {

    public static class Argument {

        enum Type {BOOLEAN, NUMERIC, STRING}

        Flag flag;
        String flagString;
        Type type;
        String shortDesc;
        String longDesc;

        boolean required;
        boolean multiple;

        int minIntValue;
        int maxIntValue;
        int defaultIntValue;

        String defaultStringValue = "";
        String[] validStringValues = null;
        String validStringValuesString = null;

        // Values
        boolean booleanValue = false;
        ArrayList<Integer> integerArrayList = new ArrayList<>();
        ArrayList<String> stringArrayList = new ArrayList<>();

        boolean set = false;


        public Argument(Flag flag,
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

        public Argument(Flag flag,
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

        public Argument(Flag flag,
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


        public Argument(Flag flag,
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
            this.validStringValues = null;


        }
        public Argument(Flag flag,
                        String flagString,
                        String shortDesc,
                        String longDesc,

                        boolean required,

                        String defaultStringValue,
                        String[] validStringValues) {
            this.flag = flag;
            this.flagString = flagString;
            this.type = Type.STRING;
            this.longDesc = longDesc;
            this.shortDesc = shortDesc;

            this.required = required;
            this.multiple = false;

            this.defaultStringValue = defaultStringValue;
            this.validStringValues = validStringValues;

            validStringValuesString = "";
            for (String validStringValue : validStringValues) {
                if (validStringValuesString.length() > 0) {
                    validStringValuesString += ", ";
                }
                validStringValuesString += validStringValue;
            }
        }



    }

    private static ArrayList<Argument> arguments = new ArrayList<>();

    public static void createCommonArguments() {
        add(new Argument(
                Flag.CIPHERTEXT,
                "i",
                "Ciphertext or ciphertext file",
                "Ciphertext string, or full path for the file with the cipher, ending with .txt.",
                false,
                ""));

        add(new Argument(
                Flag.CRIB,
                "p",
                "Crib (known-plaintext)",
                "Known plaintext (crib) at the beginning of the message.",
                false,
                ""));

        add(new Argument(
                Flag.RESOURCE_PATH,
                "r",
                "Resource directory",
                "Full path of directory for resources (e.g. stats files).",
                false,
                "."));

        add(new Argument(
                Flag.THREADS,
                "t",
                "Number of processing threads",
                "Number of threads, for multithreading. 1 for no multithreading.",
                false,
                1, 20, 7));

        add(new Argument(
                Flag.CYCLES,
                "n",
                "Number of cycles",
                "Number of cycles for key search. 0 for infinite.",
                false,
                0, 1000, 0));
    }
    public static void parseAndPrintCommandLineArgs(String[] args) {
        String[] ctArgs = CtAPI.getArgs();
        parseArguments(ctArgs, false);
        parseArguments(args, true);
        printArguments();
    }
    public static void add(Argument argument) {
        arguments.add(argument);
    }
    public static int getIntegerValue(Flag flag) {
        for (Argument argument : arguments) {
            if (argument.flag == flag) {
                if (argument.type != Argument.Type.NUMERIC) {
                    CtAPI.goodbyeError("Not a numeric flag " + flag.toString());
                }
                if (argument.multiple) {
                    CtAPI.goodbyeError("Multiple value numeric flag " + flag.toString());
                }
                if (argument.integerArrayList.isEmpty()) {
                    CtAPI.goodbyeError("No value for numeric flag " + flag.toString());
                }
                return argument.integerArrayList.get(0);
            }
        }
        CtAPI.goodbyeError("No such flag " + flag.toString());
        return -1;
    }
    public static ArrayList<Integer> getIntegerValues(Flag flag) {
        for (Argument argument : arguments) {
            if (argument.flag == flag) {
                if (argument.type != Argument.Type.NUMERIC) {
                    CtAPI.goodbyeError("Not a numeric flag " + flag.toString());
                }
                if (!argument.multiple) {
                    CtAPI.goodbyeError("Single value numeric flag " + flag.toString());
                }
                return argument.integerArrayList;
            }
        }
        CtAPI.goodbyeError("No such flag " + flag.toString());
        return null;
    }
    public static String getStringValue(Flag flag) {
        for (Argument argument : arguments) {
            if (argument.flag == flag) {
                if (argument.type != Argument.Type.STRING) {
                    CtAPI.goodbyeError("Not a string flag " + flag.toString());
                }
                if (argument.multiple) {
                    CtAPI.goodbyeError("Multiple value string flag " + flag.toString());
                }
                if (argument.stringArrayList.isEmpty()) {
                    CtAPI.goodbyeError("No value for string flag " + flag.toString());
                }
                return argument.stringArrayList.get(0);
            }
        }
        CtAPI.goodbyeError("No such flag " + flag.toString());
        return null;
    }
    public static ArrayList<String> getStringValues(Flag flag) {
        for (Argument argument : arguments) {
            if (argument.flag == flag) {
                if (argument.type != Argument.Type.STRING) {
                    CtAPI.goodbyeError("Not a string flag " + flag.toString());
                }
                if (!argument.multiple) {
                    CtAPI.goodbyeError("Single value string flag " + flag.toString());
                }
                return argument.stringArrayList;
            }
        }
        CtAPI.goodbyeError("No such flag " + flag.toString());
        return null;
    }
    public static boolean getBooleanValue(Flag flag) {
        for (Argument argument : arguments) {
            if (argument.flag == flag) {
                if (argument.type != Argument.Type.BOOLEAN) {
                    CtAPI.goodbyeError("Not a boolean flag " + flag.toString());
                }
                return argument.booleanValue;
            }
        }
        CtAPI.goodbyeError("No such flag " + flag.toString());
        return false;
    }
    public static boolean isSet(Flag flag) {
        for (Argument argument : arguments) {
            if (argument.flag == flag) {
                return argument.set;
            }
        }
        CtAPI.goodbyeError("No such flag " + flag.toString());
        return false;
    }
    public static String getShortDesc(Flag flag) {
        for (Argument mainMenuArgument : arguments) {
            if (mainMenuArgument.flag == flag) {
                return mainMenuArgument.shortDesc;
            }
        }
        return null;
    }
    public static String getFlagString(Flag flag) {
        for (Argument mainMenuArgument : arguments) {
            if (mainMenuArgument.flag == flag) {
                return mainMenuArgument.flagString;
            }
        }
        return null;
    }

    private static void parseArguments(String[] args, boolean setDefaults) {
        String error = parseArgumentsAndReturnError(args, setDefaults);
        if (error != null) {
            printUsage();
            CtAPI.goodbyeError(error);
        }
    }
    private static String parseArgumentsAndReturnError(String[] args, boolean setDefaults) {
        Argument currentArgument = null;

        for (String arg : args) {
            if (arg.toUpperCase().startsWith("-V")) {
                printUsage();
                return null;
            }
            if (arg.startsWith("-") && currentArgument != null) {
                return String.format("Invalid argument >%s<. Parameter missing for -%s (%s)\n",
                        arg, currentArgument.flagString, currentArgument.shortDesc);
            }
            if (!arg.startsWith("-") && currentArgument == null) {
                return String.format("Invalid argument >%s<.\n", arg);
            }
            if (arg.startsWith("-")) {
                currentArgument = getMainArgument(arg);
                if (currentArgument == null) {
                    return String.format("Invalid argument >%s<.\n", arg);
                }
                if (currentArgument.type == Argument.Type.BOOLEAN) {
                    currentArgument.booleanValue = true;
                    currentArgument.set = true;
                    currentArgument = null;
                }
                continue;
            }

            // Handle string or numeric values.
            if (currentArgument.type == Argument.Type.NUMERIC) {
                Integer value = null;
                try {
                    value = Integer.valueOf(arg);
                    if (value >= currentArgument.minIntValue && value <= currentArgument.maxIntValue) {
                        if (!currentArgument.multiple && currentArgument.integerArrayList.size() > 0) {
                            return String.format("Duplicate value >%s< for -%s (%s).\nPrevious value >%d<.\n",
                                    arg, currentArgument.flagString, currentArgument.shortDesc, currentArgument.integerArrayList.get(0));
                        }
                        currentArgument.integerArrayList.add(value);
                        currentArgument.set = true;
                        currentArgument = null;
                        continue;
                    } else {
                        value = null;
                    }
                } catch (NumberFormatException ignored) {
                }
                if (value == null) {
                    return String.format("Invalid value >%s< for -%s (%s). \n" +
                                    "Should be between %d and %d (default is %d).\n%s\n",
                            arg, currentArgument.flagString, currentArgument.shortDesc,
                            currentArgument.minIntValue, currentArgument.maxIntValue, currentArgument.defaultIntValue,
                            currentArgument.longDesc);
                }
            }

            if (currentArgument.type == Argument.Type.STRING) {
                if (currentArgument.stringArrayList.size() > 0 && !currentArgument.multiple) {
                    return String.format("Duplicate value >%s< for -%s (%s).\nPrevious value %s.\n",
                            arg, currentArgument.flagString, currentArgument.shortDesc, currentArgument.stringArrayList.get(0));
                }
                if (currentArgument.validStringValues != null) {
                    boolean valid = false;
                    for (String validStringValue : currentArgument.validStringValues) {
                        if (arg.equals(validStringValue)) {
                            valid = true;
                            break;
                        }

                    }
                    if (!valid) {
                        return String.format("Invalid value >%s< for -%s (%s). \n" +
                                        "Should be one of %s (default is %s).\n%s\n",
                                arg, currentArgument.flagString, currentArgument.shortDesc,
                                currentArgument.validStringValuesString, currentArgument.defaultStringValue,
                                currentArgument.longDesc);
                    }
                }

                currentArgument.stringArrayList.add(arg);
                currentArgument.set = true;
                currentArgument = null;
            }
        }

        if (currentArgument != null) {
            return String.format("Parameter missing for -%s (%s)\n", currentArgument.flagString, currentArgument.shortDesc);
        }

        if (setDefaults) {
            for (Argument arguments : arguments) {
                if (arguments.type == Argument.Type.NUMERIC && arguments.integerArrayList.isEmpty() && !arguments.multiple) {
                    if (arguments.required) {
                        return String.format("Flag -%s is mandatory but missing (%s)\n" +
                                        "Should speficiy a value between %d and %d (default is %d).\n%s\n",
                                arguments.flagString, arguments.shortDesc, arguments.minIntValue,
                                arguments.maxIntValue, arguments.defaultIntValue, arguments.longDesc);
                    } else {
                        arguments.integerArrayList.add(arguments.defaultIntValue);
                    }
                } else if (arguments.type == Argument.Type.STRING && arguments.stringArrayList.isEmpty() && !arguments.multiple) {
                    if (arguments.required) {
                        return String.format("Flag -%s is mandatory but missing (%s).\n%s\n",
                                arguments.flagString, arguments.shortDesc, arguments.longDesc);
                    } else {
                        arguments.stringArrayList.add(arguments.defaultStringValue);
                    }
                }
            }
        }
        return null;

    }
    private static void printArguments() {

        CtAPI.println("Input Parameters\n");

        for (Argument arguments : arguments) {

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
    private static Argument getMainArgument(String arg) {
        Argument currentArgument = null;
        for (Argument mainMenuArgument : arguments) {
            if (mainMenuArgument.flagString.equalsIgnoreCase(arg.substring(1))) {
                currentArgument = mainMenuArgument;
                break;
            }
        }
        return currentArgument;
    }
    private static void printUsage() {

        System.out.print("\nUsage: java -jar <jarname>.jar [arguments]\nArguments:\n");

        for (Argument currentArgument : arguments) {

            String prefix = String.format("\t-%s \t%s", currentArgument.flagString, currentArgument.shortDesc);

            switch (currentArgument.type) {
                case BOOLEAN:
                    if (currentArgument.required && currentArgument.multiple) {
                        System.out.printf("%s  (required, one or more).\n\t\t%s\n", prefix, currentArgument.longDesc);
                    } else if (currentArgument.required) {
                        System.out.printf("%s  (required).\n\t\t%s\n", prefix, currentArgument.longDesc);
                    } else if (currentArgument.multiple) {
                        System.out.printf("%s  (optional, one or more).\n\t\t%s\n", prefix, currentArgument.longDesc);
                    } else {
                        System.out.printf("%s  (optional).\n\t\t%s\n", prefix, currentArgument.longDesc);
                    }
                    break;
                case NUMERIC:
                    if (currentArgument.required && currentArgument.multiple) {
                        System.out.printf("%s \n\t\tShould specify a value between %d and %d (required, one or more).\n\t\t%s\n",
                                prefix,
                                currentArgument.minIntValue,
                                currentArgument.maxIntValue,
                                currentArgument.longDesc);
                    } else if (currentArgument.required) {
                        System.out.printf("%s \n\t\tShould specify a value between %d and %d (required).\n\t\t%s\n",
                                prefix,
                                currentArgument.minIntValue,
                                currentArgument.maxIntValue,
                                currentArgument.longDesc);
                    } else if (currentArgument.multiple) {
                        System.out.printf("%s \n\t\tShould specify a value between %d and %d (optional, one or more).\n\t\t%s\n",
                                prefix,
                                currentArgument.minIntValue, currentArgument.maxIntValue,
                                currentArgument.longDesc);
                    } else {
                        System.out.printf("%s \n\t\tShould specify a value between %d and %d (optional, default is %d).\n\t\t%s\n",
                                prefix,
                                currentArgument.minIntValue, currentArgument.maxIntValue,
                                currentArgument.defaultIntValue,
                                currentArgument.longDesc);
                    }
                    break;
                case STRING:
                    String validValuesAddition = "";
                    if (currentArgument.validStringValuesString != null && !currentArgument.validStringValuesString.isEmpty()) {
                        validValuesAddition = " \n\t\tShould specify one of " + currentArgument.validStringValuesString;
                    }
                    if (currentArgument.required && currentArgument.multiple) {
                        System.out.printf("%s %s (required, one or more).\n\t\t%s\n", prefix, validValuesAddition, currentArgument.longDesc);
                    } else if (currentArgument.required) {
                        System.out.printf("%s %s (required).\n\t\t%s\n", prefix, validValuesAddition, currentArgument.longDesc);
                    } else if (currentArgument.multiple) {
                        System.out.printf("%s %s (optional, one or more).\n\t\t%s\n", prefix, validValuesAddition, currentArgument.longDesc);
                    } else {
                        System.out.printf("%s %s (optional, default is \"%s\").\n\t\t%s\n",
                                prefix,
                                validValuesAddition,
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
