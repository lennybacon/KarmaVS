/// <reference path="../../../libs/angular/angular.js" />
/// <reference path="../../../libs/angular-mocks/angular-mocks.js" />
/// <reference path="../../../app/services/appModelDataService.js" />
/// <reference path="../../../app/directives/validate.js" />
/// <reference path="../../../libs/jasmine/jasmine-intellisense.js" />
'use strict';



function MidWayTest(x, callback) {
    
    var that = this,
        test = {},
        module,
        name;
    
    if (typeof x == 'string') {
        module = angular.module(x);
    }
    name = module.value('appName').name;
    
    var testModuleName = 'ngMidwayTest';
    
    
    this.body = function () {
        return document.getElementsByTagName('body')[0];
    };

    test.$module =
        angular.module(testModuleName, [name]).config(
            [
                '$provide',
                '$routeProvider',
                '$locationProvider',
                function ($p, $r, $l) {
                    test.$provide = $p;
                    test.$routeProvider = $r;
                    test.$locationProvider = $l;
                }
            ]
        );

    test.$module.run(
        [
            '$injector',
            function ($injector) {
                test.$injector = $injector;
            }
        ]
    );
    
    this.injector = function () {
        return test.$injector;
    };

    this.inject = function(array) {
        test.$injector.invoke(array);
    };

    angular.element(document).ready(function() {
        var body = that.body();
        var appElem = document.createElement('div');
        appElem.setAttribute('data-ng-app', testModuleName);

        body.appendChild(appElem);
        angular.bootstrap(appElem, [testModuleName]);
        that.scope = angular.element(appElem).scope();
        
        that.inject(['$controller', '$location', '$routeParams', '$rootScope', '$compile', '$filter', function ($c, $l, $p, $r, $o, $f, $v) {
            test.$controller = $c;
            test.$location = $l;
            test.$params = $p;
            test.$compile = $o;
            test.$filter = $f;
            test.$rootScope = test.$injector.get('$rootScope');
            test.$route = test.$injector.get('$route');
            if (callback) callback();
        }]);

    });

    this.scope = null;

    this.apply = function (scope, cb) {
        cb = cb || function () { };
        var s = scope || this.scope;
        if (s.$$phase) {
            cb();
        }
        else {
            s.$apply(function () {
                cb();
            });
        }
    };

    this.directive = function (html, onReady) {
        var elm = angular.element(html);
        var compiled = test.$compile(elm);
        var scope = test.$rootScope.$new();
        var element = compiled(scope);
        scope.$digest();
        if (onReady) {
            onReady(element, scope);
        }
       
        return element;
    };

    this.value = function (n, value) {
        angular.module('directiveExampleApp').value('appModel', '');
    };


    return this;
}

angular.module('directiveExampleApp', []);


describe('Midway: Directives: validate', function () {
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
        },
        test = new MidWayTest('directiveExampleApp');
    
    test.value('appModel', fakeModel);
    

    //angular.element(document).ready(function () {
    //    var testModuleName = 'directiveExampleApp';
    //    var appElem = document.createElement('div');
    //    appElem.setAttribute('data-ng-app', testModuleName);

    //    document.body.appendChild(appElem);
    //    angular.bootstrap(appElem, [testModuleName]);
    //});

    beforeEach(function () {

        
        
        //set our view html.
        html = '<input type="text" ng-model="accountNumber" data-validate="accountNumber"></input>';

    });

    //it('Should set the element attribute "required" to "required"', function () {
    //   expect(element.attr('required')).toEqual('required');
    //});

    //it('Should be invalid after focus has left without typing any key', function () {
    //    //access isolated scope
    //    element.scope().focus({ type: 'update' });
    //    element.scope().blur({ type: 'update' });
    //    expect(element.attr('class')).toEqual('ng-scope ng-pristine ng-invalid ng-invalid-required');
    //    fakeModel.metaInformation.propertyState.accountNumber.errors.push(
    //        'I\'m so invalid!');

    //    scope.$emit('response_' + 'accountNumber');

    //    element.scope().focus({ type: 'update' });
    //    element.scope().blur({ type: 'update' });
    //    expect(element.attr('class')).toEqual('ng-scope ng-pristine ng-invalid ng-invalid-required');
    //});

    it('Should be invalid after focus has left without typing any key Midway', function () {

        var e1 = test.directive(
            html,
            function (e, s) {
                    //e.scope().focus({ type: 'update' });
                    //e.scope().blur({ type: 'update' });
                    //expect(e.attr('class')).toEqual('ng-scope ng-pristine ng-invalid ng-invalid-required');
                    //fakeModel.metaInformation.propertyState.accountNumber.errors.push(
                    //    'I\'m so invalid!');

                    //s.$root.$emit('response_' + 'accountNumber');

                    //e.scope().focus({ type: 'update' });
                    //e.scope().blur({ type: 'update' });
                    //expect(e.attr('class')).toEqual('ng-scope ng-pristine ng-invalid ng-invalid-required');
        }
        );
        
        console.log(e1.scope());
        
        e1.scope().focus({ type: 'update' });
        e1.scope().blur({ type: 'update' });
        expect(e1.attr('class')).toEqual('ng-scope ng-pristine ng-invalid ng-invalid-required');
        fakeModel.metaInformation.propertyState.accountNumber.errors.push(
            'I\'m so invalid!');

        e1.scope().$emit('response_' + 'accountNumber');

        e1.scope().focus({ type: 'update' });
        e1.scope().blur({ type: 'update' });
        expect(e1.attr('class')).toEqual('ng-scope ng-pristine ng-invalid ng-invalid-required');
        
    });
});