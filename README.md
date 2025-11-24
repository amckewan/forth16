# forth16
The goal of this project is to build an 80s-era
16-bit Forth environment to play with.

There is a virtual machine, written in C, consisting of:

1. 16-bit stack-oriented integer CPU
2. 64K byte-addressed memory
3. Console I/O
4. Block-based disk I/O

Console I/O uses stdin/out. Line editing uses the `readline()`
function from the editline library (-ledit).

Blocks are stored in `blocks.fb` which is the same as gforth.

The CPU fetches 8-bit instructions from the VM's memory
and executes them with a `switch` statement.
This is the machine or assembly language.

Implemented on that is a 16-bit, indirect-threaded, Forth.

This leans on lots of prior work, but heavy reference to F83,
Starting Forth, and ideas from a 1980s polyFORTH manual.

# CPU
The CPU is a byte-code interpreter with a forth-like instruction set.
This is the assembly code used to build a 16-bit indirect-threaded Forth.

## CPU Registers
The CPU has 6 registers.
The registers are local variable in the `cpu_run()` function
and are not accessable from other functions.

# | Register | Description
--- | --- | ---
0 | T | Top of the stack
1 | S | Stack pointer, grows down
2 | R | Return-stack pointer, growns down
3 | P | Program counter
4 | I | Forth interpreter pointer
5 | W | Word pointer for ITC
