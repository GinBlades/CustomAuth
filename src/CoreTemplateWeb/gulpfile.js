var gulp = require('gulp'),
    plumber = require("gulp-plumber"),
    sourceMaps = require("gulp-sourcemaps"),
    tsc = require("gulp-typescript"),
    tsLint = require("gulp-tslint"),
    concat = require("gulp-concat"),
    sass = require("gulp-sass"),
    uglify = require("gulp-uglify");

var styles = ["ClientSource/StyleSheet/**/*.scss"];
var scripts = ["ClientSource/JavaScript/**/*.ts"];

gulp.task("tsc", function () {
    var tsProject = tsc.createProject("tsconfig.json");
    var tsResult = gulp.src(scripts)
        .pipe(plumber())
        .pipe(tsLint({ formatter: "prose", configuration: "tslint.json" }))
        .pipe(tsLint.report({ emitError: false }))
        .pipe(sourceMaps.init())
        .pipe(tsProject());

    return tsResult.js
        .pipe(concat("app.js"))
        .pipe(uglify())
        .pipe(sourceMaps.write("."))
        .pipe(gulp.dest("wwwroot/js"));
});

gulp.task("copySourceMaps", function () {
    gulp.src([
        "node_modules/jquery/dist/jquery.min.map"
    ])
    .pipe(gulp.dest("wwwroot/js"));
})

gulp.task("vendorJs", ["copySourceMaps"], function () {
    gulp.src([
        "node_modules/jquery/dist/jquery.min.js",
        "node_modules/bootstrap-sass/assets/javascripts/bootstrap.min.js"
    ])
        .pipe(concat("vendor.js"))
        .pipe(gulp.dest("wwwroot/js"));
});

gulp.task("copyFonts", function () {
    return gulp.src("node_modules/bootstrap-sass/assets/fonts/bootstra/*.*")
        .pipe(gulp.dest("wwwroot/fonts"));
});

gulp.task("sass", function () {
    return gulp.src(styles)
        .pipe(plumber())
        .pipe(sourceMaps.init())
        .pipe(sass({ style: "compressed" }))
        .pipe(sourceMaps.write("."))
        .pipe(gulp.dest("wwwroot/css"));
});

gulp.task("watch", function () {
    gulp.watch(styles, ["sass"]);
    gulp.watch(scripts, ["tsc"]);
});

gulp.task("build", ["copyFonts", "vendorJs", "sass", "tsc"]);

gulp.task('default', ["build", "watch"]);