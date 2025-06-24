// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    $('.js-example-basic-single').select2({
        placeholder: "Select",
        width: '100%'
    });
    $('.js-example-basic-single').on('select2:open', function (e) {
        $('.select2-search__field').attr('placeholder', 'Search...');
    });
});