// A program to convert Forth block files to and from
// a text format (.fx) that works reasonably well with git.
//
// Andrew McKewan, 25-Nov-2025
//
// Usage:
//    $ blk2txt e  < blocks.fb     > blocks.fb.fx  # encode
//    $ blk2txt d  < blocks.fb.fx  > blocks.fb     # decode
//
// A block is written to the .fx file in one of three ways:
//    B blk   Blank block
//    Z blk   Zero block
//    S blk   Screen block, followed by 16 lines
//            non-printable lines are base64 encoded
//
// blk is written for documentation, the decoder
// is stream-based (no lseek) and ignores it.
//

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

int is_text(const char *line, int len) { // true if all text
  for (int i = 0; i < len; i++) {
    if (line[i] < 32 || line[i] > 126)
      return 0;
  }
  return 1;
}

int is_filled_with(const char *block, char c) {
  for (int i = 0; i < 1024; i++) {
    if (block[i] != c)
      return 0;
  }
  return 1;
}

int encode_block(const char *block) {
  const char *line = block;
  for (int ln = 0; ln < 16; ln++, line+=64) {
    if (is_text(line, 64)) {
      print_text(line);
    } else {
      print_base64(line);
    }
    putchar('\n');
  }
  return 0;
}

// Encode stdin 1024 bytes at a time (a block) and encode to
// stdout as printable ASCII. Non-text lines are encoded
// in base64.
int encode_stdin() {
  char block[1024];
  int blk = 0;
  char fill = -1;
  while (fread(block, sizeof block, 1, stdin) == 1) {
    if (is_filled_with(block, ' ')) {
      if (fill != ' ') {
        printf("B %d\n", blk);
        fill = ' ';
      }
    } else if (is_filled_with(block, 0)) {
      if (fill != 0) {
        printf("Z %d\n", blk);
        fill = 0;
      }
    } else {
      printf("S %d\n", blk);
      encode_block(block);
      fill = -1;
    }
    blk++;
  }
  printf("E %d\n", blk);
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

int trim(char *line) { // remove trailing newline, return len
  int len = strlen(line);
  if (len && line[len-1] == '\n') line[--len] = 0;
  return len;
}

// read 16 lines, check for base64 encoding, write 1024 bytes
int decode_block() {
  for (int ln=0; ln<16; ln++) {
    char line[128];
    unsigned char out[64+2]; // +2 for base64 padding
    if (!fgets(line, sizeof line, stdin)) return -1;
    int len = trim(line);
    if (len > 64) { // base64 (88 chars)
      decode_line(line, out);
    } else { // plain text
      memset(out, ' ', 64);
      memcpy(out, line, len);
    }
    fwrite(out, 1, 64, stdout);
  }
  return 0;
}

void write_filled_block(char c) {
  char block[1024];
  memset(block, c, sizeof block);
  fwrite(block, sizeof block, 1, stdout);
}

void write_missing_blocks(int blk, int scr, char fill) {
  while (blk++ < scr) {
    write_filled_block(fill);
  }
}

// Decode text from stdin into a Forth block file at stdout.
int decode_stdin() {
  char line[128];
  int last = -1;
  char fill = ' ';
  while (fgets(line, sizeof line, stdin)) {
    char code; int blk;
    if (sscanf(line, "%c %d", &code, &blk) != 2) {
      fprintf(stderr, "Invalid line: %s", line);
      return -1;
    }    
    while (++last < blk) { // write missing blocks
      write_filled_block(fill);
    }
    switch (code) {
      case 'B': write_filled_block(fill = ' '); break;
      case 'Z': write_filled_block(fill = 0); break;
      case 'S': decode_block(); break;
      case 'E': return 0;
      default:
        fprintf(stderr, "Invalid code: %s", line);
        return -1;
    }
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
