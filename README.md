# RDF XtraReport RTL Text Fixer
As of now (v15.1) DevExpress XtraReport engine doesn't support RTL text rendering so when you have a text of mixed Latin and Farsi characters, the alignments of indivisual words are incorrect.
In order to temporarily fix this, we provided a fixer library such that it will automatically fix any given XtraReport by just a simple method call.

## Usage
Pretty simple, just call FixRTLText method on a given XtraReport object and with drill down to every possible text control and applies the fix
```
var xtraReport = myReport as XtraReport;

xtraReport.FixRTLText();
```
