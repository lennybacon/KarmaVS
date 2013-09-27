'use strict';

angular.module('directiveExampleApp')
	.directive('albums', function() {
	return {
		templateUrl: 'views/templates/albums.html',
		restrict: 'E',
		scope: {
			title: '@title'
		}
	};
});