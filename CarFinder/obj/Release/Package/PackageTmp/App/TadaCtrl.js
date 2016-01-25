
//2. References the initialized module as a variable
var tadaListApp = angular.module('tadaListApp'); 

//Defines the controller functionality for the module
tadaListApp.controller('tadaListController', function () {
        var tl = this; //defines your SCOPE FROM HERE FORWARD
        tl.tadas = []; //an array literal.

        //record total of incompleted todos
        tl.remainingTadas = function () {
            var count = 0;
            angular.forEach(tl.tadas, function (tada) {
                count += tada.done ? 0 : 1; // adds 1 to the count for each tada that's not done
            });
            return count;
        };

        //enter a todo list item
        //hit enter to add item
        tl.newTada = function () {
            console.log(tl.tadaText);
            tl.tadas.push({ text:tl.tadaText, done:false});
            tl.tadaText = '';
        };

        //click an item to mark complete
        tl.markComplete = function (tada) {
            tada.done = !tada.done;                
        };

        //click 'clear completed' to dump the completed tasks
        tl.clearCompleted = function (tada) {
            tl.tadas = tl.tadas.filter(function (item) {
                return !item.done;
            })
        }

});

