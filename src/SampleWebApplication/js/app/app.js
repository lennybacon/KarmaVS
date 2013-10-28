'use strict';

angular.module('directiveExampleApp', [])
  .value('appModel', {
      model: {
          accountNumber: ''
      },
      metaInformation: {
          propertyState: {
              'accountNumber': {
                  //initial
                  required: {
                      expression: 'true',
                      message: 'This is a required field'
                  },
                  //only after post
                  errors: [], //error messages
              }
          }
      }
  })
  .config(function ($routeProvider) {
    $routeProvider
      .when('/', {
        templateUrl: 'views/main.html',
        controller: 'MainCtrl'
      })
      .otherwise({
        redirectTo: '/'
      });
  });
