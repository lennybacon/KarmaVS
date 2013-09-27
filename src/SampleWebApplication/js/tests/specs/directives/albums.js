'use strict';

describe('Directive: albums', function() {
	beforeEach(module('directiveExampleApp'));

	var element, scope;

	beforeEach(module('views/templates/albums.html'));

	beforeEach(inject(function($rootScope, $compile) {
		element = angular.element('<div class="well span6">' + 
			'<h3>Busdriver Albums:</h3>' +
			'<albums ng-repeat="album in albums" title="{{album.title}}">' + 
			'</albums></div>');

		scope = $rootScope;

		scope.albums = [{
			'title': 'Memoirs of the Elephant Man'
		}, {
			'title': 'Temporary Forever'  
		}, {
			'title': 'Cosmic Cleavage'
		}, {
			'title': 'Fear of a Black Tangent'
		}, {
			'title': 'RoadKillOvercoat'
		}, {
			'title': 'Jhelli Beam'
		}, {
			'title': 'Beaus$Eros'
		}];

		$compile(element)(scope);
		scope.$digest();
	}));

	it("should have the correct amount of albums in the list", function() {
		var list = element.find('li');
		expect(list.length).toBe(7);
		
	});

	it("should display the correct album title for the first item in the albums list", function() {
		var list = element.find('li');
		expect(list.eq(0).text()).toBe('Memoirs of the Elephant Man');
	});
});