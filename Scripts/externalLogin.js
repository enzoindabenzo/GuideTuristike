// Scripts/externalLogin.js
document.getElementById('externalLoginForm').addEventListener('submit', function (e) {
    console.log('Form submitting with provider: ' + e.submitter.value);
});