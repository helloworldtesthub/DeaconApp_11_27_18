/* Settings for the CKEditor */




CKEDITOR.replace("editor1", {
    language: "en",
    //uiColor: "#9AB8F3",
    height: 300,
    width: "auto",
    toolbarCanCollapse: true,


    // Define the toolbar.
    toolbar: [
        { name: 'clipboard', items: ['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'Undo', 'Redo'] },
        { name: 'editing', items: ['SpellChecker'] },
        { name: 'links', items: ['Link', 'Unlink'] },
        { name: 'insert', items: ['Table', 'HorizontalRule', 'SpecialChar'] },
        { name: 'tools', items: ['Maximize'] },
        { name: 'document', items: ['Source'] },
        '/',
        { name: 'basicstyles', items: ['Bold', 'Italic', 'Strike', 'Superscript', 'Subscript', '-', 'RemoveFormat'] },
        { name: 'paragraph', items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote'] },
        { name: 'styles', items: ['Styles', 'Format'] },
        { name: 'colors', items: ["TextColor", "BGColor"] },
        { name: 'about', items: ['About'] }
    ],
    // Remove the redundant buttons from toolbar items defined above.
    removeButtons: "Underline,Strike,Subscript,Superscript,Anchor,Styles,Specialchar,Image"
});
