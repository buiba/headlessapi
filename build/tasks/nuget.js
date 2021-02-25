"use strict";

const gulp = require("gulp"),
    path = require("path"),
    getVersion = require("./get-version"),
    { pack } = require('gulp-dotnet-cli');
let tasks = (buildHelper) => {
    gulp.task("nuget-contentdelivery", () => {
        return gulp.src([
            '**/*.ContentApi.Cms.csproj',
            '**/*.ContentApi.Commerce.csproj',
            '**/*.ContentApi.Core.csproj',
            '**/*.ContentApi.OAuth.csproj',
            '**/*.ContentApi.Search.csproj',
            '**/*.ContentApi.Search.Commerce.csproj',
            '**/*.ContentDeliveryApi.csproj',
            '**/*.ContentApi.Forms.csproj',
            '**/*.Sample.csproj'

        ], { read: false })
            .pipe(pack({
                output: path.join(process.cwd(), 'nupkgs'),
                version: getVersion("./build/version.props").packageVersion,
                configuration: buildHelper.configuration,
                noBuild: true,
                noRestore: true
            }));
     });

     gulp.task("nuget-contentmanagement", () => {
        return gulp.src([
          '**/*.DefinitionsApi.csproj',
          '**/*.DefinitionsApi.Commerce.csproj',
          '**/*.ContentManagementApi.csproj'

        ], { read: false })
            .pipe(pack({
                output: path.join(process.cwd(), 'nupkgs'),
                version: getVersion("./build/content-management-version.props").packageVersion,
                configuration: buildHelper.configuration,
                noBuild: true,
                noRestore: true
            }));
     });

     gulp.task("nuget", gulp.parallel("nuget-contentdelivery", "nuget-contentmanagement"));

};

module.exports = tasks;
