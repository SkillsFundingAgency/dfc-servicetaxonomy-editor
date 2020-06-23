/// <binding ProjectOpened='watch' />
var gulp = require('gulp');

gulp.task('default', function () {
    var sass = require('gulp-dart-sass');
    var sourcemaps = require('gulp-sourcemaps');
    var csso = require('gulp-csso');
    var rename = require('gulp-rename');

    return gulp.src('./wwwroot/Styles/*.scss')
        .pipe(sourcemaps.init())
        .pipe(sass())
        .pipe(sourcemaps.write())
        .pipe(gulp.dest('wwwroot/Styles/'))
        .pipe(csso())
        .pipe(rename(function (path) {
            return {
                dirname: path.dirname,
                basename: path.basename,
                extname: ".min.css"
            }
        }))
        .pipe(gulp.dest('./wwwroot/Styles/'));
});

gulp.task('watch', gulp.series('default', function () {
    return gulp.watch('./wwwroot/Styles/*.scss', gulp.series('default'));
}));
