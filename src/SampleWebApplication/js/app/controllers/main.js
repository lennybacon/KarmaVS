'use strict';

angular.module('directiveExampleApp')
  .controller('MainCtrl', function ($scope, appModelDataService) {
      $scope.foo = 42;
      $scope.model = appModelDataService.getModel('');
      $scope.users = [
        {
            "name": "Obi-Wan \"Ben\" Kenobi",
            "snippet": "Obi-Wan Kenobi, later known as Ben Kenobi during his exile, was a legendary Jedi Master who played a significant role in the fate of the galaxy during the waning days of the Galactic Republic. He was the mentor of both Anakin and Luke, training both in the ways of the Force. He had a long and tumultuous career that helped shape the fate of the entire galaxy."
        },
        {
            "name": "Luke Skywalker",
            "snippet": "Born in 19 BBY as the son of the fallen Jedi Knight Anakin Skywalker and the Queen and Senator of Naboo, Padmé Amidala, Luke was raised on Tatooine and hidden from Emperor Palpatine and his father, who had become Darth Vader, Dark Lord of the Sith. In 0 BBY, Skywalker's life changed forever. A chance purchase of two droids, R2-D2 and C-3PO, led to him to receive training in the ways of the Force from Jedi Master Obi-Wan and to meet Han, and Princess Leia Organa, who was, unbeknownst to him, his twin sister. Skywalker then destroyed the first Death Star and joined the Rebel Alliance."
        },
        {
            "name": "Han Solo",
            "snippet": "Han Solo was a Human smuggler from the manufacturing planet Corellia who achieved galactic fame as a member of the Rebel Alliance and later the New Republic. Born on Corellia, he was orphaned at an early age and taken by the pirate Garris Shrike to serve on his crew. He was treated cruelly, and served Shrike for many years before escaping while in his teens. Solo became a smuggler, and fell in love with Bria Tharen, though she left him due to her duties to the Rebel Alliance. Solo then entered the Imperial Academy at Carida, serving with distinction. He was kicked out, however, when he stopped an Imperial officer from beating a Wookiee named Chewbacca with a neuronic whip for resisting capture."
        }
      ];
  });
