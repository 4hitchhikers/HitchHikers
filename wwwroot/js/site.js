// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


    jQuery(document).ready(function() {

        jQuery('#usa').vectorMap(

        {

            map: 'usa_en',
            backgroundColor: 'white',
            borderColor: '#FF9900',
            borderOpacity: 0.60,
            borderWidth: 2,
            color: '#1076C8',
            hoverColor: '#0A4C82',
            selectedColor: '#FF9900',

        }

    );

});
