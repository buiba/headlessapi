"use strict";

const getVersion = require("./tasks/get-version");

class BuildHelper {
  constructor(configuration) {
    this.configuration = configuration;
    this.version = this._getVersion();
  }

  _getVersion() {
    var version = getVersion();
    return version.packageVersion;
  }
}

module.exports = BuildHelper;
