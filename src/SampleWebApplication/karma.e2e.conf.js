module.exports = function (config) {
    config.set({
        // Test frameworks to use
        frameworks: ['ng-scenario', 'jasmine'],

        // Base path, that will be used to resolve files and exclude
        basePath: '.',

        // List of files / patterns to load in the browser
        files: [
          { pattern: 'app/**/*.html', watched: true, included: false, served: false },
          'js/tests/e2e/**/*.js'
        ],

        urlRoot: '/_karma_/',
        
        proxies: {
            '/': 'http://localhost:52473/'
        },


        // Preprocessors to convert e.g. html to angular template cache items
        preprocessors: {
            'views/**/*.html': 'ng-html2js'
        },

        // List of files to exclude
        exclude: [],

        // Test results reporter to use possible values: dots, growl, junit, ...
        reporters: ['progress'],

        // Web server port
        port: 8080,

        // CLI runner port
        runnerPort: 9100,

        // Enable / disable colors in the output (reporters and logs)
        colors: true,

        // Level of logging: LOG_DISABLE, LOG_ERROR, LOG_WARN, LOG_INFO, LOG_DEBUG
        logLevel: config.LOG_DEBUG,

        // Enable / disable watching file and execute tests on changes
        autoWatch: true,

        // Start these browsers, currently available:
        // - Chrome
        // - ChromeCanary
        // - Firefox
        // - Opera
        // - Safari (only Mac)
        // - PhantomJS
        // - IE (only Windows)
        browsers: ['PhantomJS'],

        // If browser does not capture in given timeout [ms], kill it
        captureTimeout: 5000,

        // Continuous Integration mode
        // if true, it capture browsers, run tests and exit
        singleRun: false,

        // Plugins
        plugins: [
          'karma-jasmine',
          'karma-ng-scenario',
          'karma-ng-html2js-preprocessor',
          'Karma-phantomjs-launcher',
          'karma-chrome-launcher',
          'karma-firefox-launcher',
          'karma-junit-reporter'
        ]
    });
}