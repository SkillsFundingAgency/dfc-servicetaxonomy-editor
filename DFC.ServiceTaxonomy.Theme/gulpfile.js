/// <binding ProjectOpened='watch' />
var gulp = require('gulp');

gulp.task('css', function () {
    var sass = require('gulp-dart-sass');
    var sourcemaps = require('gulp-sourcemaps');

    return gulp.src('./wwwroot/Styles/trumbowyg_scoped_govuk_frontend.scss')
        .pipe(sourcemaps.init())
        .pipe(sass())
        .pipe(sourcemaps.write())
        .pipe(gulp.dest('wwwroot/Styles/'));
});

gulp.task('watch', gulp.series('css', function () {
    return gulp.watch('./wwwroot/Styles/trumbowyg_scoped_govuk_frontend.scss', gulp.series('css'));
}));

gulp.task('default', gulp.series('css', function () {
    var csso = require('gulp-csso');
    var rename = require('gulp-rename');

    return gulp.src('./wwwroot/Styles/trumbowyg_scoped_govuk_frontend.css')
        .pipe(csso())
        .pipe(rename('trumbowyg_scoped_govuk_frontend.min.css'))
        .pipe(gulp.dest('./wwwroot/Styles/'));
}));
