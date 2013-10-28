/// <reference path="../../../libs/angular/angular.js" />
/// <reference path="../../../libs/angular-mocks/angular-mocks.js" />
/// <reference path="../../../app/app.js" />
/// <reference path="../../../app/services/appModelDataService.js" />
/// <reference path="../../../app/directives/validate.js" />
/// <reference path="../../../libs/jasmine/jasmine-intellisense.js" />
'use strict';

describe('Unit: Directives: validate', function () {
    var scope,
        element,
        compiled,
        html,
        fakeModel = {
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
        };

    beforeEach(function () {

        module('directiveExampleApp', function ($provide) {
            $provide.value(
                'appModel',
                fakeModel
            );
        });

        //set our view html.
        html = '<input type="text" ng-model="accountNumber" data-validate="accountNumber"></input>';

        inject(function ($compile, $rootScope) {
            //create a scope (you could just use $rootScope, I suppose)
            scope = $rootScope.$new();

            //get the jqLite or jQuery element
            element = angular.element(html);

            //compile the element into a function to process the view.
            compiled = $compile(element);

            //run the compiled view.
            compiled(scope);

            //call digest on the scope!
            scope.$digest();
        });
    });

    //it('Should set the element attribute "required" to "required"', function () {
    //   expect(element.attr('required')).toEqual('required');
    //});
    
    it('Should be invalid after focus has left without typing any key', function () {
        //access isolated scope
        expect(element.attr('class')).toEqual('ng-scope ng-pristine ng-valid');
        element.scope().focus({ type: 'update' });
        element.scope().blur({ type: 'update' });
        expect(element.attr('class')).toEqual('ng-scope ng-pristine ng-invalid ng-invalid-required');
        element.scope().focus({ type: 'update' });
        element.scope().accountNumber = '42';
        element.scope().$digest();
        element.scope().blur({ type: 'update' });
        expect(element.attr('class')).toEqual('ng-scope ng-pristine ng-valid ng-valid-required');

        //fakeModel.metaInformation.propertyState.accountNumber.errors.push(
        //    'I\'m so invalid!');
        
        //scope.$emit('response_' + 'accountNumber');
        //expect(element.attr('class')).toEqual('ng-scope ng-pristine ng-invalid ng-invalid-required');
        //element.scope().focus({ type: 'update' });
        //element.scope().blur({ type: 'update' });
        //expect(element.attr('class')).toEqual('ng-scope ng-pristine ng-invalid ng-invalid-required');
    });
});