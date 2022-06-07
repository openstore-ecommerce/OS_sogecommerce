

$(document).ready(function () {

    $('#OS_sogecommerce_cmdSave').unbind("click");
    $('#OS_sogecommerce_cmdSave').click(function () {
        $('.processing').show();
        $('.actionbuttonwrapper').hide();
        // lower case cmd must match ajax provider ref.
        nbxget('os_sogecommerce_savesettings', '.OS_sogecommercedata', '.OS_sogecommercereturnmsg');
    });

    $('.selectlang').unbind("click");
    $(".selectlang").click(function () {
        $('.editlanguage').hide();
        $('.actionbuttonwrapper').hide();
        $('.processing').show();
        $("#nextlang").val($(this).attr("editlang"));
        // lower case cmd must match ajax provider ref.
        nbxget('os_sogecommerce_selectlang', '.OS_sogecommercedata', '.OS_sogecommercedata');
    });


    $(document).on("nbxgetcompleted", OS_sogecommerce_nbxgetCompleted); // assign a completed event for the ajax calls

    // function to do actions after an ajax call has been made.
    function OS_sogecommerce_nbxgetCompleted(e) {

        $('.processing').hide();
        $('.actionbuttonwrapper').show();
        $('.editlanguage').show();

        if (e.cmd == 'os_sogecommerce_selectlang') {
                        
        }

    };

});

