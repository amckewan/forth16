// A program to convert Forth block files to and from
// a text format that works well with git.
//
// Andrew McKewan, 25-Nov-2025
//
// $ blk2txt e < blocks.fb > blocks.fb.txt
// $ blk2txt d < blocks.fb.txt > blocks.fb

#include <stdio.h>
#include <string.h>

// base64 encode from https://base64.sourceforge.net/b64.c
// encode 3 8-bit binary bytes as 4 base64 characters
void encode(const unsigned char *in, char *out, int len ) {
  static const char digit[] =
    "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
  out[0] = digit[in[0] >> 2];
  out[1] = digit[((in[0] & 0x03) << 4) | ((in[1] & 0xf0) >> 4)];
  out[2] = len > 1 ? digit[((in[1] & 0x0f) << 2) | ((in[2] & 0xc0) >> 6)] : '=';
  out[3] = len > 2 ? digit[in[2] & 0x3f] : '=';
}

void print_base64(const char *line) { // prints 88 chars
  const unsigned char *in = (const unsigned char *) line;
  for (int len = 64; len > 0; len -= 3, in += 3) {
    char out[4];
    encode(in, out, len);
    fwrite(out, sizeof out, 1, stdout);
  }
}

void print_text(const char* line) {
  int len = 64;
  while (len > 0 && line[len-1] == ' ') --len;
  fwrite(line, 1, len, stdout);
}

int is_ascii(const char *line, int len) {
  for (int i = 0; i < len; i++) {
    if (line[i] < 32 || line[i] > 126)
      return 0;
  }
  return 1;
}

// Encode stdin 64 bytes at a time (a line from a block) to
// stdout as ASCII. Non-ASCII lines are encoded in base64.
int encode_stdin() {
  char line[64];
  while (fread(line,1,64,stdin) == 64) {
    if (is_ascii(line, 64))
      print_text(line);
    else
      print_base64(line);
    putchar('\n');
  }
  return 0;
}

unsigned char decode_char(char c) {
  if (c >= 'A' && c <= 'Z') return c - 'A';
  if (c >= 'a' && c <= 'z') return c - 'a' + 26;
  if (c >= '0' && c <= '9') return c - '0' + 52;
  if (c == '+') return 62;
  if (c == '/') return 63;
  return 0; // e.g. '='
}

// decode 4 base64 characters into 3 8-bit binary bytes
void decode(const char *line, unsigned char *out) {
  unsigned char in[4];
  for (int i = 0; i < 4; i++) in[i] = decode_char(line[i]);
  out[0] = (in[0] << 2 | in[1] >> 4);
  out[1] = (in[1] << 4 | in[2] >> 2);
  out[2] = (((in[2] << 6) & 0xc0) | in[3]);
}

// in: 88 chars base64 encoding, out: 64 bytes
void decode_line(const char *line, unsigned char *out) {
  for (int i = 0; i < 64; i += 3, line += 4, out += 3) {
    decode(line, out);
  }
}

// Decode text from stdin into a Forth block file at stdout.
int decode_stdin() {
  char line[90]; // 88 + \n
  unsigned char out[66]; // +2 for padding
  while (fgets(line, sizeof line, stdin)) {
    int len = strlen(line);
    if (len && line[len-1] == '\n') line[--len] = 0;
    if (len > 64) { // base64 (88 chars)
      decode_line(line, out);
    } else {
      memset(out, ' ', 64);
      memcpy(out, line, len);
    }
    fwrite(out, 1, 64, stdout);
  }
  return 0;
}

int main(int argc, char *argv[]) {
  char arg = argc > 1 ? argv[1][0] : '?';
  if (arg == 'e') {
    return encode_stdin();
  } else if (arg == 'd') {
    return decode_stdin();
  } else {
    fprintf(stderr, "blk2txt 'e' or 'd'\n");
    return 1;
  }
}
