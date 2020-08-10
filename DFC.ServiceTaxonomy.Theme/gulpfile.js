/// <binding ProjectOpened='watch' />
var gulp = require('gulp'); 

gulp.task('clean', function () {
    var del = require('del');
    
    return del('./wwwroot/Fonts/**', { force: true });
});

gulp.task('fonts', gulp.series('clean', function () {
    return gulp.src('./node_modules/nationalcareers_toolkit/assets/dist/fonts/*')
        .pipe(gulp.dest('./wwwroot/Fonts/'));
}));

gulp.task('default', gulp.series('fonts', function () {
    var sass = require('gulp-dart-sass');
    var sourcemaps = require('gulp-sourcemaps');
    var csso = require('gulp-csso');
    var rename = require('gulp-rename');
    var replace = require('gulp-replace');

    return gulp.src('./wwwroot/Styles/*.scss')
        .pipe(sourcemaps.init())
        .pipe(sass())
        .pipe(sourcemaps.write())
        .pipe(replace(/nationalcareers_toolkit\/fonts\//g, 'DFC.ServiceTaxonomy.Theme/Fonts/'))
        .pipe(gulp.dest('wwwroot/Styles/'))
        .pipe(csso({
            restructure: false
        }))
        .pipe(rename(function (path) {
            return {
                dirname: path.dirname,
                basename: path.basename,
                extname: ".min.css"
            }
        }))
        .pipe(gulp.dest('./wwwroot/Styles/'));
}));

gulp.task('watch', gulp.series('default', function () {
    return gulp.watch('./wwwroot/Styles/*.scss', gulp.series('default'));
}));
