/* eslint-env es6, node */
"use strict";
const BuildHelper = require("./build/buildHelper"),
    program = require("commander");

program
    .option("--configuration <configuration>", "Set the build configuration", /^(debug|release)$/i, "debug")
    .parse(process.argv);


// Create the bulid helper
let buildHelper = new BuildHelper(program.configuration);

// Import all build tasks
require("./build/tasks/set-version")(process.argv);
require("./build/tasks/nuget")(buildHelper);
