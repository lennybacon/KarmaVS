'use strict';

describe(
    'StarWars App',
    function () {
        describe(
            'Star wars user list view',
            function () {

                beforeEach(
                    function () {
                        browser().navigateTo('/app/index.html');
                    }
                );

                it(
                    'should filter the list as user types into the search box',
                    function () {
                        expect(repeater('.users li').count()).toBe(3);

                        input('query').enter('obi-wan kenobi'); 
                        expect(repeater('.users li').count()).toBe(1);

                        input('query').enter('Han');
                        expect(repeater('.users li').count()).toBe(2);
                        
                        input('query').enter('');
                        expect(repeater('.users li').count()).toBe(3);
                    }
                );
            }
        );
    }
);
