"use strict";

const gulp = require("gulp"),
    gutil = require("gulp-util"),
    program = require("commander"),
    xmlpoke = require("xmlpoke"),
    getVersion = require("./get-version"),
    tsm = require("teamcity-service-messages");

module.exports = () => {
    gulp.task("set-version", function (done) {

        if (process.argv.indexOf("--build") === -1) {
            gutil.log(gutil.colors.cyan("Parameter '--build' is required"));
            throw new Error();
        }
        if (process.argv.indexOf("--jirabranch") === -1) {
            gutil.log(gutil.colors.cyan("Parameter '--jirabranch' is required"));
            throw new Error();
        }
        program
            .option("--build <build>", "Build number")
            .option("--jirabranch <jirabranch>", "Branch name")
            .parse(process.argv);

        var preReleaseInfo = "";
        const branchName = program.jirabranch.toLocaleLowerCase();

        let issueId;
        issueId = (issueId = /([a-z]+-[0-9]+)/.exec(branchName)) ? `${issueId[0]}-` : "";

        if (branchName === "master" || branchName.startsWith("master-")) {
            preReleaseInfo = "";
        } else if (branchName.startsWith("release")) {
            preReleaseInfo = "pre-";
        } else if (branchName === "develop") {
            preReleaseInfo = "inte-";
        } else if (branchName.startsWith("feature")) {
            preReleaseInfo = `feature-${issueId}`;
        } else {
            preReleaseInfo = "developerbuild-";
        }


        if (preReleaseInfo) {
            preReleaseInfo += program.build.padStart(6, "0");
        }

        xmlpoke(["./build/version.props", "./build/content-management-version.props"], (xml) => {
            xml.set("//Project/PropertyGroup/VersionSuffix", preReleaseInfo);

        });

        tsm.setParameter({
            name: "packageVersion",
            value: getVersion().packageVersion
        });

        done();
    });
};
