'use strict';

angular.module('directiveExampleApp')
	.directive(
	    'something',
	    function() {
	        // this is an attribute with no required controllers, 
	        // and no isolated scope, so we're going to use all the
	        // defaults, and just providing a linking function.

	        return function(scope, elem, attrs) {

	            scope.click = function() {
	                console.log('new value: ' + attrs.something);
	                var newValue = scope.$eval(attrs.something);
	                console.log('new value: ' + newValue);
	                elem.val(newValue);
	            };

	            scope.focus = function () {
	                console.log('focus');
	            };

	            elem.bind('click', scope.click);
	            elem.bind('focus', scope.focus);
	        };
	    }
	);