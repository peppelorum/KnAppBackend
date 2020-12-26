var gulp = require('gulp'),
    sass = require('gulp-sass')
    cssmin = require("gulp-cssmin")
    rename = require("gulp-rename");

var imagemin = require('gulp-imagemin');

gulp.task('min', function (done) {
    gulp.src('assets/scss/style.scss')
        .pipe(sass().on('error', sass.logError))
        .pipe(cssmin())
        .pipe(rename({
            suffix: ".min"
        }))
        .pipe(gulp.dest('wwwroot/assets/css'));
    done();
    gulp.src('node_modules/leaflet/dist/leaflet.js')
    .pipe(gulp.dest('wwwroot/assets/leaflet'));
    gulp.src('node_modules/leaflet/dist/leaflet.css')
    .pipe(gulp.dest('wwwroot/assets/leaflet'));
    gulp.src('node_modules/leaflet/dist/images/**/*.+(png|jpg|gif|svg)')
    .pipe(imagemin())
    .pipe(gulp.dest('wwwroot/assets/leaflet/images'));
    gulp.src('node_modules/axios/dist/axios.min.js')
    .pipe(gulp.dest('wwwroot/assets/js'));

    gulp.src('node_modules/leaflet.markercluster/dist/leaflet.markercluster.js')
    .pipe(gulp.dest('wwwroot/assets/leaflet'))
    
    gulp.src('node_modules/leaflet.markercluster/dist/MarkerCluster.css')
    .pipe(gulp.dest('wwwroot/assets/leaflet'));

    gulp.src('node_modules/leaflet.markercluster/dist/MarkerCluster.Default.css')
    .pipe(gulp.dest('wwwroot/assets/leaflet'));
});

gulp.task('js', async function(done) {
    gulp.src('assets/js/*.js')
    .pipe(gulp.dest('wwwroot/assets/js'));
});

gulp.task('watch', function(){
    gulp.watch('assets/js/*.js', gulp.series(['js'])); 
});

gulp.task("serve", gulp.parallel(['min','js']));
gulp.task("default", gulp.series("serve"));
