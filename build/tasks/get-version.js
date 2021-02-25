"use strict";

const fs = require("fs");

module.exports = (versionPath) => {
      versionPath = versionPath ? versionPath : "./build/version.props";

      const data = fs.readFileSync(versionPath, "utf-8"),
        prefix = data.match("<VersionPrefix>(.+?)<\/VersionPrefix>")[1],
        suffixMatch = data.match("<VersionSuffix>(.+?)<\/VersionSuffix>"),
        suffix = suffixMatch ? `-${suffixMatch[1]}` : "";
    return {
        version: prefix,
        packageVersion: `${prefix}${suffix}`
    };
};
