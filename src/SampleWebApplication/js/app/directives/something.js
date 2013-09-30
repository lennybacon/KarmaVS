'use strict';

angular.module('directiveExampleApp')
	.directive(
	    'something',
	    function() {
	        // this is an attribute with no required controllers, 
	        // and no isolated scope, so we're going to use all the
	        // defaults, and just providing a linking function.

	        return function(scope, elem, attrs) {
	            elem.bind('click', function () {
	                var newValue = scope.$eval(attrs.validate);
	                console.log(newValue);
	                elem.val(newValue);
	            });
	        };
	    }
	);