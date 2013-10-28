/// <reference path="../../../libs/angular/angular.js" />
/// <reference path="../../../libs/angular-mocks/angular-mocks.js" />
/// <reference path="../../../app/app.js" />
/// <reference path="../../../app/directives/something.js" />
/// <reference path="../../../libs/jasmine/jasmine-intellisense.js" />

'use strict';

describe('Directive something', function () {
    var scope,
        elem,
        compiled,
        html;

    beforeEach(function () {
        //load the module
        module('directiveExampleApp');

        //set our view html.
        html = '<input data-something="foo" type="text" data-ng-model="text">';

        inject(function ($compile, $rootScope) {
            //create a scope (you could just use $rootScope, I suppose)
            scope = $rootScope.$new();
            scope.text = '';
            scope.foo = '42';
            //get the jqLite or jQuery element
            elem = angular.element(html);

            //compile the element into a function to 
            // process the view.
            compiled = $compile(elem);

            //run the compiled view.
            compiled(scope);

            //call digest on the scope!
            scope.$digest();
        });
    });

    it('should have an empty value.', function () {
        expect(elem.val()).toBe('');
    });
    
    it('should have the value of the text property of the scope.', function () {
        scope.text = 'bar';
        scope.$digest();
        expect(elem.val()).toBe('bar');
    });
    
    it('should have the value of the foo property of the scope.', function () {
        console.log(elem.scope());
        elem.scope().focus();
        expect(elem.val()).toBe('');
    });

    it('should have the value of the foo property of the scope.', function () {
        elem[0].click();
        expect(elem.val()).toBe('42');
    });
});